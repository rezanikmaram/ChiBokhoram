/**
 * Voyage SEO Detail Page JavaScript Module
 * Handles filtering, SEO selection, and performance statistics
 */

const VoyageSeoDetail = (function() {
    'use strict';

    // =====================================================
    // STATE MANAGEMENT
    // =====================================================
    const state = {
        selectedSeoId: null,
        filters: {
            conflict: false,
            incomplete: false,
            noTali: false
        }
    };

    // =====================================================
    // DOM ELEMENTS CACHE
    // =====================================================
    const elements = {};

    function cacheElements() {
        elements.seoCards = document.querySelectorAll('.seo-card');
        elements.productRows = document.querySelectorAll('#productsTable tbody tr');
        elements.userCards = document.querySelectorAll('.user-performance-card');
        
        // Use Array.from to ensure we get all cards including hidden ones
        elements.equipmentCards = Array.from(document.querySelectorAll('.equipment-performance-card'));
        elements.truckCards = Array.from(document.querySelectorAll('.truck-performance-card'));
        
        // Force refresh on each applyFilters call for equipment and truck cards
        // to handle dynamic showing/hiding
        elements.equipmentCards = Array.from(document.querySelectorAll('.equipment-performance-card'));
        elements.truckCards = Array.from(document.querySelectorAll('.truck-performance-card'));
        
        elements.filterConflict = document.getElementById('filterConflict');
        elements.filterIncomplete = document.getElementById('filterIncomplete');
        elements.filterNoTali = document.getElementById('filterNoTali');
        
        elements.filterConflictLabel = document.getElementById('filterConflictLabel');
        elements.filterIncompleteLabel = document.getElementById('filterIncompleteLabel');
        elements.filterNoTaliLabel = document.getElementById('filterNoTaliLabel');
        
        elements.selectedSeoIndicator = document.getElementById('selectedSeoIndicator');
        elements.selectedSeoText = document.getElementById('selectedSeoText');
        elements.productCountBadge = document.getElementById('productCountBadge');
        elements.userCountBadge = document.getElementById('userCountBadge');
        elements.equipmentCountBadge = document.getElementById('equipmentCountBadge');
        elements.truckCountBadge = document.getElementById('truckCountBadge');
        
        elements.noResultsMessage = document.getElementById('noResultsMessage');
        elements.userPerformanceNote = document.getElementById('userPerformanceNote');
        elements.equipmentPerformanceNote = document.getElementById('equipmentPerformanceNote');
        elements.truckPerformanceNote = document.getElementById('truckPerformanceNote');
    }

    // =====================================================
    // SEO SELECTION
    // =====================================================
    function selectSeo(seoId, seoName) {
        // Toggle off if already selected
        if (state.selectedSeoId === seoId) {
            clearSeoSelection();
            return;
        }

        state.selectedSeoId = seoId;
        updateSeoCardSelection(seoId);
        updateSelectedSeoIndicator(seoName);
        applyFilters();
    }

    function clearSeoSelection() {
        state.selectedSeoId = null;
        elements.seoCards.forEach(card => card.classList.remove('is-selected'));
        
        if (elements.selectedSeoIndicator) {
            elements.selectedSeoIndicator.classList.add('is-hidden');
        }
        
        applyFilters();
    }

    function updateSeoCardSelection(seoId) {
        elements.seoCards.forEach(card => {
            card.classList.remove('is-selected');
            if (card.dataset.seoId === seoId) {
                card.classList.add('is-selected');
            }
        });
    }

    function updateSelectedSeoIndicator(seoName) {
        if (elements.selectedSeoIndicator && elements.selectedSeoText) {
            elements.selectedSeoIndicator.classList.remove('is-hidden');
            elements.selectedSeoText.textContent = seoName;
        }
    }

    // =====================================================
    // FILTERING
    // =====================================================
    function updateFilterState() {
        state.filters.conflict = elements.filterConflict?.checked ?? false;
        state.filters.incomplete = elements.filterIncomplete?.checked ?? false;
        state.filters.noTali = elements.filterNoTali?.checked ?? false;

        // Update label styles
        elements.filterConflictLabel?.classList.toggle('is-active', state.filters.conflict);
        elements.filterIncompleteLabel?.classList.toggle('is-active', state.filters.incomplete);
        elements.filterNoTaliLabel?.classList.toggle('is-active', state.filters.noTali);
    }

    function applyFilters() {
        updateFilterState();
        
        const visibleProductIds = filterProducts();
        updateProductCount(visibleProductIds.length);
        
        const visibleSeoIds = extractVisibleSeoIds();
        
        recalculateUserPerformance(visibleSeoIds);
        filterEquipmentPerformance(visibleSeoIds);
        filterTruckPerformance(visibleSeoIds);
        
        updateEmptyState(visibleProductIds.length === 0);
    }

    function filterProducts() {
        const visibleIds = [];
        const isAnyFilterActive = isFilteringActive();

        elements.productRows.forEach(row => {
            const shouldShow = shouldShowProductRow(row, isAnyFilterActive);
            
            if (shouldShow) {
                row.classList.remove('is-hidden');
                visibleIds.push(row.dataset.seoId);
            } else {
                row.classList.add('is-hidden');
            }
        });

        return visibleIds;
    }

    function shouldShowProductRow(row, isAnyFilterActive) {
        const seoId = row.dataset.seoId;
        const hasConflict = row.dataset.conflict === 'true';
        const isIncomplete = row.dataset.incomplete === 'true';
        const hasNoTali = row.dataset.notali === 'true';

        // Check SEO selection filter
        if (state.selectedSeoId && seoId !== state.selectedSeoId) {
            return false;
        }

        // Check other filters
        if (isAnyFilterActive) {
            const matchesConflict = state.filters.conflict && hasConflict;
            const matchesIncomplete = state.filters.incomplete && isIncomplete;
            const matchesNoTali = state.filters.noTali && hasNoTali;
            
            if (!(matchesConflict || matchesIncomplete || matchesNoTali)) {
                return false;
            }
        }

        return true;
    }

    function isFilteringActive() {
        return state.filters.conflict || state.filters.incomplete || state.filters.noTali;
    }

    function extractVisibleSeoIds() {
        const seoIds = new Set();
        
        // Get all product rows that are NOT hidden
        const allRows = document.querySelectorAll('#productsTable tbody tr');
        
        allRows.forEach(row => {
            // Only include rows that aren't hidden by filters
            if (!row.classList.contains('is-hidden')) {
                const seoId = row.dataset.seoId;
                if (seoId) {
                    seoIds.add(seoId.toLowerCase());
                }
            }
        });
        
        return seoIds;
    }

    function updateProductCount(count) {
        if (elements.productCountBadge) {
            elements.productCountBadge.textContent = count;
        }
    }

    function updateEmptyState(isEmpty) {
        if (elements.noResultsMessage) {
            elements.noResultsMessage.classList.toggle('is-hidden', !isEmpty);
        }
    }

    // =====================================================
    // USER PERFORMANCE
    // =====================================================
    function recalculateUserPerformance(visibleSeoIds) {
        const userStats = calculateUserStatsFromVisibleRows();
        updateUserCards(userStats, visibleSeoIds);
    }

    function calculateUserStatsFromVisibleRows() {
        const visibleRows = document.querySelectorAll('#productsTable tbody tr:not(.is-hidden)');
        const stats = {};

        visibleRows.forEach(row => {
            const taliBlocks = row.querySelectorAll('td:nth-child(9) .tali-block');
            
            taliBlocks.forEach(block => {
                processTaliBlock(block, stats);
            });
        });

        return stats;
    }

    function processTaliBlock(block, stats) {
        const isDock = block.classList.contains('tali-block--dock');
        const isArea = block.classList.contains('tali-block--area');
        
        const personnelEl = block.querySelector('.tali-block__meta');
        if (!personnelEl) return;
        
        const name = personnelEl.textContent.split('|')[0].trim();
        if (!name || name.includes('فایل') || name.includes('چک')) return;

        if (!stats[name]) {
            stats[name] = createEmptyUserStats();
        }

        const count = extractCountFromBlock(block);
        const problemCount = extractProblemCountFromBlock(block);
        const hasFile = block.querySelector('.icon-file:not(.is-missing)') !== null;
        const hasChecklist = block.querySelector('.icon-checklist:not(.is-missing)') !== null;

        if (isDock) {
            updateDockStats(stats[name], count, problemCount, hasFile, hasChecklist);
        } else if (isArea) {
            updateAreaStats(stats[name], count, problemCount, hasFile, hasChecklist);
        }
    }

    function createEmptyUserStats() {
        return {
            dockCount: 0, dockProblem: 0, dockTali: 0, dockFile: 0, dockCheck: 0,
            areaCount: 0, areaProblem: 0, areaTali: 0, areaFile: 0, areaCheck: 0
        };
    }

    function extractCountFromBlock(block) {
        const countText = block.querySelector('.tali-block__count')?.textContent || '';
        const countMatch = countText.match(/:\s*(\d+)/);
        return countMatch ? parseInt(countMatch[1]) : 0;
    }

    function extractProblemCountFromBlock(block) {
        const problemBadge = block.querySelector('.voyage-badge--warning');
        return problemBadge ? parseInt(problemBadge.textContent.replace(/\D/g, '')) || 0 : 0;
    }

    function updateDockStats(stats, count, problemCount, hasFile, hasChecklist) {
        stats.dockCount += count;
        stats.dockProblem += problemCount;
        stats.dockTali += 1;
        if (hasFile) stats.dockFile += 1;
        if (hasChecklist) stats.dockCheck += 1;
    }

    function updateAreaStats(stats, count, problemCount, hasFile, hasChecklist) {
        stats.areaCount += count;
        stats.areaProblem += problemCount;
        stats.areaTali += 1;
        if (hasFile) stats.areaFile += 1;
        if (hasChecklist) stats.areaCheck += 1;
    }

    function updateUserCards(userStats, visibleSeoIds) {
        // Get fresh reference to all user cards from DOM
        const allUserCards = document.querySelectorAll('.user-performance-card');
        
        let visibleCount = 0;
        const isFiltering = isFilteringActive() || state.selectedSeoId;

        allUserCards.forEach(card => {
            const userName = card.dataset.userName;
            const stats = userStats[userName];

            if (stats) {
                updateUserCardContent(card, stats);
                card.classList.remove('is-hidden', 'is-inactive');
                visibleCount++;
            } else if (isFiltering) {
                card.classList.add('is-hidden');
            } else {
                card.classList.remove('is-hidden');
                card.classList.add('is-inactive');
                visibleCount++;
            }
        });

        updateUserCountBadge(visibleCount, isFiltering);
    }

    function updateUserCardContent(card, stats) {
        // Dock stats
        updateElementText(card, '.dock-count', stats.dockCount.toLocaleString());
        updateElementText(card, '.dock-tali', stats.dockTali);
        updateElementText(card, '.dock-file', stats.dockFile);
        updateElementText(card, '.dock-check', stats.dockCheck);
        toggleElementVisibility(card, '.dock-problem', stats.dockProblem);
        updateElementText(card, '.dock-problem', stats.dockProblem);

        // Area stats
        updateElementText(card, '.area-count', stats.areaCount.toLocaleString());
        updateElementText(card, '.area-tali', stats.areaTali);
        updateElementText(card, '.area-file', stats.areaFile);
        updateElementText(card, '.area-check', stats.areaCheck);
        toggleElementVisibility(card, '.area-problem', stats.areaProblem);
        updateElementText(card, '.area-problem', stats.areaProblem);

        // Totals
        const totalCount = stats.dockCount + stats.areaCount;
        const totalProblem = stats.dockProblem + stats.areaProblem;
        const totalTali = stats.dockTali + stats.areaTali;
        const totalFile = stats.dockFile + stats.areaFile;
        const totalCheck = stats.dockCheck + stats.areaCheck;

        updateElementText(card, '.total-count', totalCount.toLocaleString());
        updateElementText(card, '.total-tali', totalTali);
        updateElementText(card, '.total-file', totalFile);
        updateElementText(card, '.total-check', totalCheck);
        toggleElementVisibility(card, '.total-problem', totalProblem);
        updateElementText(card, '.total-problem', totalProblem);
    }

    function updateUserCountBadge(count, isFiltering) {
        if (elements.userCountBadge) {
            elements.userCountBadge.textContent = count;
        }

        if (elements.userPerformanceNote) {
            if (isFiltering) {
                elements.userPerformanceNote.textContent = `مربوط به کالاهای فیلتر شده (${count} کاربر فعال)`;
                elements.userPerformanceNote.classList.add('text-primary', 'fw-bold');
            } else {
                elements.userPerformanceNote.textContent = 'مربوط به تمام کالاها';
                elements.userPerformanceNote.classList.remove('text-primary', 'fw-bold');
            }
        }
    }

    function filterEquipmentPerformance(visibleSeoIds) {
        // Get fresh reference to all equipment cards from DOM
        const allEquipmentCards = document.querySelectorAll('.equipment-performance-card');
        
        let visibleCount = 0;
        const isFiltering = isFilteringActive() || state.selectedSeoId;

        allEquipmentCards.forEach(card => {
            const relatedSeoIds = parseRelatedSeoIds(card.dataset.seoIds);
            
            let shouldShow = false;
            
            if (isFiltering) {
                shouldShow = shouldShowPerformanceCard(relatedSeoIds, visibleSeoIds, true);
            } else {
                shouldShow = true;
            }
            
            if (shouldShow) {
                card.classList.remove('is-hidden', 'is-inactive');
                visibleCount++;
            } else {
                card.classList.add('is-hidden');
            }
        });

        updateEquipmentCountBadge(visibleCount, isFiltering);
    }

    // =====================================================
    // TRUCK PERFORMANCE
    // =====================================================
    function filterTruckPerformance(visibleSeoIds) {
        // Get fresh reference to all truck cards from DOM
        const allTruckCards = document.querySelectorAll('.truck-performance-card');
        
        let visibleCount = 0;
        const isFiltering = isFilteringActive() || state.selectedSeoId;

        allTruckCards.forEach(card => {
            const relatedSeoIds = parseRelatedSeoIds(card.dataset.seoIds);
            
            let shouldShow = false;
            
            if (isFiltering) {
                shouldShow = shouldShowPerformanceCard(relatedSeoIds, visibleSeoIds, true);
            } else {
                shouldShow = true;
            }
            
            if (shouldShow) {
                card.classList.remove('is-hidden', 'is-inactive');
                visibleCount++;
            } else {
                card.classList.add('is-hidden');
            }
        });

        updateTruckCountBadge(visibleCount, isFiltering);
    }

    function shouldShowPerformanceCard(relatedSeoIds, visibleSeoIds, isFiltering) {
        if (!isFiltering) return true;
        if (!visibleSeoIds || visibleSeoIds.size === 0) return false;
        
        // If no related SEO IDs (empty from server), show the card anyway
        if (!relatedSeoIds || relatedSeoIds.length === 0) {
            return true;
        }
        
        return relatedSeoIds.some(id => visibleSeoIds.has(id.toLowerCase()));
    }

    function parseRelatedSeoIds(seoIdsString) {
        return seoIdsString ? seoIdsString.split(',').filter(id => id) : [];
    }

    function updateTruckCardIfNeeded(card, truckPlate) {
        // This function can be extended to recalculate truck stats from visible rows
        // For now, it just ensures the card is visible
    }

    function resetTruckCard(card) {
        const originalCount = card.dataset.originalCount;
        
        if (originalCount) {
            updateElementText(card, '.truck-visit-count', originalCount);
            updateElementText(card, '.truck-dock-count', originalCount);
        }
    }

    function updateEquipmentCountBadge(count, isFiltering) {
        if (elements.equipmentCountBadge) {
            elements.equipmentCountBadge.textContent = count;
        }

        if (elements.equipmentPerformanceNote) {
            if (isFiltering) {
                elements.equipmentPerformanceNote.textContent = `مربوط به کالاهای فیلتر شده (${count} تجهیز)`;
                elements.equipmentPerformanceNote.classList.add('text-primary', 'fw-bold');
            } else {
                elements.equipmentPerformanceNote.textContent = 'مربوط به تمام کالاها';
                elements.equipmentPerformanceNote.classList.remove('text-primary', 'fw-bold');
            }
        }
    }

    function updateTruckCountBadge(count, isFiltering) {
        if (elements.truckCountBadge) {
            elements.truckCountBadge.textContent = count;
        }

        if (elements.truckPerformanceNote) {
            if (isFiltering) {
                elements.truckPerformanceNote.textContent = `مربوط به کالاهای فیلتر شده (${count} کامیون)`;
                elements.truckPerformanceNote.classList.add('text-primary', 'fw-bold');
            } else {
                elements.truckPerformanceNote.textContent = 'وزن بار قبل از تجهیز - اسکله';
                elements.truckPerformanceNote.classList.remove('text-primary', 'fw-bold');
            }
        }
    }

    // =====================================================
    // UTILITY FUNCTIONS
    // =====================================================
    function updateElementText(parent, selector, text) {
        const el = parent.querySelector(selector);
        if (el) el.textContent = text;
    }

    function toggleElementVisibility(parent, selector, shouldShow) {
        const el = parent.querySelector(selector);
        if (el) {
            el.style.display = shouldShow ? '' : 'none';
        }
    }

    function showCard(card) {
        card.classList.remove('is-hidden');
        card.classList.remove('is-inactive');
    }

    function hideCard(card) {
        card.classList.add('is-hidden');
    }

    // =====================================================
    // CLEAR ALL FILTERS
    // =====================================================
    function clearAllFilters() {
        if (elements.filterConflict) elements.filterConflict.checked = false;
        if (elements.filterIncomplete) elements.filterIncomplete.checked = false;
        if (elements.filterNoTali) elements.filterNoTali.checked = false;
        clearSeoSelection();
    }

    // =====================================================
    // INITIALIZATION
    // =====================================================
    function init() {
        cacheElements();
        applyFilters();
        
        // Expose global functions for inline event handlers
        window.selectSeo = selectSeo;
        window.clearSeoSelection = clearSeoSelection;
        window.applyFilters = applyFilters;
        window.clearAllFilters = clearAllFilters;
    }

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Public API
    return {
        selectSeo,
        clearSeoSelection,
        applyFilters,
        clearAllFilters
    };
})();
