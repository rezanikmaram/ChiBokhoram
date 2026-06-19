function changeToNumber(dataText, idText, id, decimalPlaces = 2, isNullable = false) {
    let originalValue = dataText.value;
    let value = originalValue.replaceAll(',', '');

    // If cleared completely and nullable, send empty
    if (value === '' && isNullable) {
        document.getElementById(idText).value = '';
        document.getElementById(id).value = '';
        return;
    }

    let hasDot = value.includes('.');
    let parts = value.split('.');
    let integerPart = parts[0].replace(/\D/g, '');
    if (integerPart === '') integerPart = '';

    let formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");

    let decimalPart = '';
    if (parts.length > 1) {
        decimalPart = parts[1].replace(/\D/g, '').slice(0, decimalPlaces);
        if (decimalPart.length > 0) {
            decimalPart = '.' + decimalPart;
        } else if (hasDot) {
            decimalPart = '.';
        }
    }

    if (integerPart === '0' && decimalPart === '') {
        formattedInteger = '0';
    }

    let formattedValue = formattedInteger + decimalPart;
    document.getElementById(idText).value = formattedValue;

    let hiddenValue = integerPart;
    if (decimalPart.startsWith('.') && decimalPart.length > 1) {
        hiddenValue = integerPart + decimalPart;
    }
    document.getElementById(id).value = hiddenValue;
}

function formatOnBlur(dataText, idText, id, decimalPlaces = 2, isNullable = false) {
    let value = dataText.value.replaceAll(',', '');
    if (value === '' || value === '.') {
        document.getElementById(idText).value = '';
        document.getElementById(id).value = isNullable ? '' : '0';
        return;
    }

    if (value.endsWith('.')) {
        value = value.slice(0, -1);
    }

    let numericValue = parseFloat(value);
    if (isNaN(numericValue)) {
        document.getElementById(idText).value = '';
        document.getElementById(id).value = isNullable ? '' : '0';
        return;
    }

    if (numericValue === 0) {
        document.getElementById(idText).value = '0';
        document.getElementById(id).value = '0';
        return;
    }

    let parts = numericValue.toString().split('.');
    let integerPart = parts[0];
    let formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");

    let decimalPart = '';
    if (parts.length > 1) {
        let decimals = parts[1];
        decimals = decimals.replace(/0+$/, '');
        if (decimals.length > 0) {
            decimalPart = '.' + decimals;
        }
    }

    let formattedValue = formattedInteger + decimalPart;
    document.getElementById(idText).value = formattedValue;
    document.getElementById(id).value = numericValue;
}

function SetNumber(dataText, id) {
    if (dataText === null || dataText === undefined || dataText === '') {
        id = id.replace('#', '');
        let idText = id + 'PriceText';
        let inputEl = document.getElementById(id);
        let textEl = document.getElementById(idText);

        if (!textEl || !inputEl) {
            console.warn('SetNumber: Element not found for', id);
            return;
        }

        let isNullable = inputEl.getAttribute('data-nullable') === 'true';
        if (isNullable) {
            textEl.value = '';
            inputEl.value = '';
        } else {
            textEl.value = '0';
            inputEl.value = '0';
        }
        return;
    }

    id = id.replace('#', '');
    let idText = id + 'PriceText';

    let textEl = document.getElementById(idText);
    let inputEl = document.getElementById(id);

    if (!textEl || !inputEl) {
        console.warn('SetNumber: Element not found for', id);
        return;
    }

    let value = dataText.toString().replaceAll(',', '');

    let numericValue = parseFloat(value);
    if (isNaN(numericValue)) {
        let isNullable = inputEl.getAttribute('data-nullable') === 'true';
        textEl.value = '';
        inputEl.value = isNullable ? '' : '0';
        return;
    }

    if (numericValue === 0) {
        textEl.value = '0';
        inputEl.value = '0';
        return;
    }

    let parts = numericValue.toString().split('.');
    let integerPart = parts[0];
    let formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");

    let decimalPart = '';
    if (parts.length > 1) {
        let decimals = parts[1];
        decimals = decimals.replace(/0+$/, '');
        if (decimals.length > 0) {
            decimalPart = '.' + decimals;
        }
    }

    let formattedValue = formattedInteger + decimalPart;
    textEl.value = formattedValue;
    inputEl.value = numericValue;
}

function restrictToNumberAndDecimal(event) {
    // Allow: backspace, delete, tab, escape, enter
    if ([46, 8, 9, 27, 13].includes(event.keyCode) ||
        // Allow: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
        (event.keyCode === 65 && event.ctrlKey === true) ||
        (event.keyCode === 67 && event.ctrlKey === true) ||
        (event.keyCode === 86 && event.ctrlKey === true) ||
        (event.keyCode === 88 && event.ctrlKey === true) ||
        // Allow: home, end, left, right
        (event.keyCode >= 35 && event.keyCode <= 39)) {
        return;
    }

    // Ensure that it's a number or decimal point
    if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
        // Check if it's a decimal point and if we don't already have one
        if (event.keyCode === 190 || event.keyCode === 110) {
            if (event.target.value.includes('.')) {
                event.preventDefault();
            }
            return;
        }
        event.preventDefault();
    }
}

function cleanNumberInputs() {
    $('input.clean-number').each(function () {
        let value = $(this).val();
        let isNullable = $(this).attr('data-nullable') === 'true';

        if (value === null || value === undefined || value.trim() === '') {
            $(this).val(isNullable ? '' : '0');
            return;
        }

        let numericValue = parseFloat(value.replaceAll(',', ''));
        if (isNaN(numericValue)) {
            $(this).val(isNullable ? '' : '0');
            return;
        }

        if (numericValue === 0) {
            $(this).val("0");
            return;
        }

        let parts = numericValue.toString().split('.');
        let integerPart = parts[0];
        let formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ",");

        let decimalPart = '';
        if (parts.length > 1) {
            let decimals = parts[1];
            decimals = decimals.replace(/0+$/, '');
            if (decimals.length > 0) {
                decimalPart = '.' + decimals;
            }
        }

        $(this).val(formattedInteger + decimalPart);
    });
}

$(document).ready(function () {
    cleanNumberInputs();

    $('input[type="text"][onkeyup*="changeToNumber"]').on('keydown', function(event) {
        restrictToNumberAndDecimal(event);
    });

    $('input[type="text"][onkeyup*="changeToNumber"]').on('blur', function() {
        let onkeyupAttr = $(this).attr('onkeyup');
        if (onkeyupAttr) {
            let match = onkeyupAttr.match(/changeToNumber\(this,\s*'([^']+)',\s*'([^']+)',?\s*(\d*),?\s*(true|false)\)/);
            if (match) {
                let idText = match[1];
                let id = match[2];
                let decimalPlaces = match[3] ? parseInt(match[3]) : 2;
                let isNullable = match[4] === 'true';
                formatOnBlur(this, idText, id, decimalPlaces, isNullable);
            }
        }
    });
});