/**
 * Voyage Report JavaScript Module
 * Handles interactive features for the voyage reporting system
 */

const VoyageReport = (function() {
    'use strict';

    // Private variables
    let currentPage = 1;
    let isLoading = false;

    // Private methods
    const initialize = function() {
        bindEvents();
        initializeAnimations();
        initializeTooltips();
    };

    const bindEvents = function() {
        // Form submission handling
        const filterForm = document.getElementById('filterForm');
        if (filterForm) {
            filterForm.addEventListener('submit', handleFilterSubmit);
        }

        // Search input debouncing
        const searchInput = document.querySelector('input[name="Filter.Search"]');
        if (searchInput) {
            let timeout;
            searchInput.addEventListener('input', function(e) {
                clearTimeout(timeout);
                timeout = setTimeout(() => {
                    handleSearchInput(e.target.value);
                }, 500);
            });
        }

        // Date change handlers
        const dateInputs = document.querySelectorAll('.persian-date');
        dateInputs.forEach(input => {
            input.addEventListener('change', handleDateChange);
        });

        // Export button handling
        const exportBtn = document.querySelector('button[form="exportForm"]');
        if (exportBtn) {
            exportBtn.addEventListener('click', handleExport);
        }

        // Pagination handling
        document.addEventListener('click', function(e) {
            if (e.target.closest('.pagination .page-link')) {
                e.preventDefault();
                const href = e.target.closest('.page-link').getAttribute('href');
                if (href) {
                    handlePagination(href);
                }
            }
        });

        // Voyage card click handling
        document.addEventListener('click', function(e) {
            const voyageCard = e.target.closest('.voyage-card');
            if (voyageCard && !e.target.closest('button') && !e.target.closest('a')) {
                const voyageId = voyageCard.dataset.voyageId;
                if (voyageId) {
                    handleVoyageCardClick(voyageId);
                }
            }
        });
    };

    const handleFilterSubmit = function(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const params = new URLSearchParams(formData);
        
        // Update URL without page reload
        const newUrl = window.location.pathname + '?' + params.toString();
        window.history.pushState({}, '', newUrl);
        
        // Show loading state
        showLoadingState();
        
        // Simulate loading (in real app, this would be an AJAX call)
        setTimeout(() => {
            hideLoadingState();
        }, 800);
    };

    const handleSearchInput = function(searchTerm) {
        if (searchTerm.length < 2) {
            return;
        }

        // Debounced search
        console.log('Searching for:', searchTerm);
        // In real implementation, this would make an AJAX call
    };

    const handleDateChange = function(e) {
        const dateFrom = document.querySelector('input[name="Filter.DateFromFa"]').value;
        const dateTo = document.querySelector('input[name="Filter.DateToFa"]').value;
        
        if (dateFrom && dateTo) {
            // Validate date range
            if (!isValidDateRange(dateFrom, dateTo)) {
                showNotification('تاریخ پایان باید بعد از تاریخ شروع باشد', 'error');
                e.target.value = '';
                return;
            }
        }
    };

    const handleExport = function(e) {
        e.preventDefault();
        const form = document.getElementById('exportForm');
        
        // Show loading state
        const btn = e.target.closest('button');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i>در حال آماده‌سازی...';
        btn.disabled = true;
        
        // Submit form
        setTimeout(() => {
            form.submit();
        }, 500);
    };

    const handlePagination = function(href) {
        showLoadingState();
        window.location.href = href;
    };

    const handleVoyageCardClick = function(voyageId) {
        // Add ripple effect
        event.currentTarget.style.transform = 'scale(0.98)';
        setTimeout(() => {
            event.currentTarget.style.transform = '';
        }, 150);
    };

    const initializeAnimations = function() {
        // Intersection Observer for scroll animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.classList.add('animate-in');
                    }, index * 100);
                }
            });
        }, observerOptions);

        // Observe elements
        document.querySelectorAll('.voyage-card, .stat-card, .seo-card').forEach(el => {
            observer.observe(el);
        });
    };

    const initializeTooltips = function() {
        // Initialize tooltips for operation indicators
        const indicators = document.querySelectorAll('.indicator');
        indicators.forEach(indicator => {
            indicator.addEventListener('mouseenter', function() {
                const tooltip = document.createElement('div');
                tooltip.className = 'custom-tooltip';
                tooltip.textContent = this.getAttribute('title') || '';
                document.body.appendChild(tooltip);
                
                const rect = this.getBoundingClientRect();
                tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
                tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';
                
                this._tooltip = tooltip;
            });
            
            indicator.addEventListener('mouseleave', function() {
                if (this._tooltip) {
                    this._tooltip.remove();
                    delete this._tooltip;
                }
            });
        });
    };

    const isValidDateRange = function(dateFrom, dateTo) {
        // Simple validation - in real app, use proper Persian date comparison
        return dateFrom <= dateTo;
    };

    const showLoadingState = function() {
        isLoading = true;
        const loader = document.createElement('div');
        loader.id = 'voyage-report-loader';
        loader.className = 'voyage-report-loader';
        loader.innerHTML = `
            <div class="loader-content">
                <div class="spinner"></div>
                <p>در حال بارگذاری...</p>
            </div>
        `;
        document.body.appendChild(loader);
    };

    const hideLoadingState = function() {
        isLoading = false;
        const loader = document.getElementById('voyage-report-loader');
        if (loader) {
            loader.remove();
        }
    };

    const showNotification = function(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `voyage-report-notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fa-solid fa-${type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                <span>${message}</span>
                <button class="close-btn" onclick="this.parentElement.parentElement.remove()">
                    <i class="fa-solid fa-times"></i>
                </button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    };

    const updateStatistics = function(data) {
        // Update statistics cards with animation
        const statCards = document.querySelectorAll('.stat-value');
        statCards.forEach((card, index) => {
            const finalValue = data[index];
            animateValue(card, 0, finalValue, 1000);
        });
    };

    const animateValue = function(element, start, end, duration) {
        const range = end - start;
        const increment = range / (duration / 16);
        let current = start;
        
        const timer = setInterval(() => {
            current += increment;
            if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
                element.textContent = end.toLocaleString('fa-IR');
                clearInterval(timer);
            } else {
                element.textContent = Math.floor(current).toLocaleString('fa-IR');
            }
        }, 16);
    };

    // Public API
    return {
        init: initialize,
        updateStatistics: updateStatistics,
        showNotification: showNotification,
        showLoading: showLoadingState,
        hideLoading: hideLoadingState
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    VoyageReport.init();
});

// Add CSS for loading and notifications
const additionalCSS = `
.voyage-report-loader {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
}

.loader-content {
    background: white;
    padding: 2rem;
    border-radius: 15px;
    text-align: center;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
}

.spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #667eea;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 1rem;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

.voyage-report-notification {
    position: fixed;
    top: 20px;
    right: 20px;
    background: white;
    border-radius: 10px;
    box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
    z-index: 9999;
    min-width: 300px;
    animation: slideInRight 0.3s ease-out;
}

.voyage-report-notification.error {
    border-right: 4px solid #e74a3b;
}

.voyage-report-notification.info {
    border-right: 4px solid #36b9cc;
}

.notification-content {
    padding: 1rem;
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.notification-content i {
    font-size: 1.2rem;
}

.voyage-report-notification.error .notification-content i {
    color: #e74a3b;
}

.voyage-report-notification.info .notification-content i {
    color: #36b9cc;
}

.close-btn {
    background: none;
    border: none;
    color: #858796;
    cursor: pointer;
    padding: 0.25rem;
    margin-right: auto;
}

.close-btn:hover {
    color: #5a5c69;
}

@keyframes slideInRight {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

.custom-tooltip {
    position: absolute;
    background: #2c3e50;
    color: white;
    padding: 0.5rem 0.75rem;
    border-radius: 6px;
    font-size: 0.8rem;
    white-space: nowrap;
    z-index: 1000;
    pointer-events: none;
}

.custom-tooltip::before {
    content: '';
    position: absolute;
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    border: 6px solid transparent;
    border-top-color: #2c3e50;
}
`;

// Inject additional CSS
const style = document.createElement('style');
style.textContent = additionalCSS;
document.head.appendChild(style);
