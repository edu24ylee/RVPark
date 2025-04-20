let originalLotTypeId, originalLotId;

$(document).ready(function () {
    originalLotTypeId = $('#Reservation_LotTypeId').val();
    originalLotId = $('#Reservation_LotId').val();

    $('#Reservation_LotId').prop('disabled', true).val(originalLotId);
    updateCostsOnly();

    $('#Reservation_StartDate, #Reservation_EndDate, #Rv_Length').on('change', updateCostsOnly);
    $('#Reservation_LotId').on('change', updateCostsOnly);

    $('#toggleManualFees').on('change', function () {
        $('#manualFeeTableContainer').toggle(this.checked);
        updateCostsOnly();
    });

    $(document).on('change', 'input[name="SelectedManualFees"]', updateCostsOnly);

    $('#Reservation_LotTypeId').one('click', function () {
        $('#Reservation_LotId').prop('disabled', false);
        updateLotsAndCosts();
    });

    $('#Reservation_LotTypeId').on('change', function () {
        $('#Reservation_LotId').prop('disabled', false);
        updateLotsAndCosts();
    });
});

function updateLotsAndCosts() {
    const lotTypeId = $('#Reservation_LotTypeId').val();
    const trailerLength = $('#Rv_Length').val();
    const startDate = $('#Reservation_StartDate').val();
    const endDate = $('#Reservation_EndDate').val();

    updateDuration(startDate, endDate);

    if (!lotTypeId || !trailerLength || !startDate || !endDate) return;

    $.get('/api/reservation/available-lots', {
        lotTypeId,
        trailerLength,
        startDate,
        endDate
    }, function (response) {
        const lots = response.data;
        const $lotSelect = $('#Reservation_LotId');
        $lotSelect.empty();

        if (!lots.length) {
            $lotSelect.append('<option disabled selected>No lots available</option>');
        } else {
            $lotSelect.append('<option disabled selected>-- Select a Lot --</option>');
            lots.forEach(lot => {
                const rate = parseFloat(lot.lotTypeRate).toFixed(2);
                $lotSelect.append(`<option value="${lot.id}" data-rate="${rate}">Lot #${lot.id} - ${lot.location}</option>`);
            });
        }

        updateCostsOnly();
    }).fail(() => {
        console.error("Failed to load available lots.");
    });
}

function updateDuration(startDateStr, endDateStr) {
    const start = new Date(startDateStr);
    const end = new Date(endDateStr);
    const duration = Math.max(0, Math.ceil((end - start) / (1000 * 60 * 60 * 24)));
    $('#Reservation_Duration').val(duration || '');
}

function updateCostsOnly() {
    const start = $('#Reservation_StartDate').val();
    const end = $('#Reservation_EndDate').val();
    updateDuration(start, end);

    const duration = parseInt($('#Reservation_Duration').val()) || 0;
    const rate = parseFloat($('#Reservation_LotId option:selected').data('rate')) || 0;
    const manualFee = getManualFeeTotal();
    const taxRate = parseFloat($('#TaxRate').val()) || 0.0825;

    const baseTotal = duration * rate;
    const taxable = baseTotal + manualFee;
    const tax = taxable * taxRate;
    const totalDue = baseTotal + manualFee + tax;

    const amountPaid = parseFloat($('#amountPaidValue').val()) || 0;
    const remainingBalance = Math.max(0, totalDue - amountPaid);

    $('#baseTotalText').text(baseTotal.toFixed(2));
    $('#manualFeeText').text(manualFee.toFixed(2));
    $('#taxAmountText').text(tax.toFixed(2));
    $('#grandTotalText').text(totalDue.toFixed(2));
    $('#amountPaidText').text(amountPaid.toFixed(2));
    $('#remainingBalanceText').text(remainingBalance.toFixed(2));
}

function getManualFeeTotal() {
    let total = 0;
    $('input[name="SelectedManualFees"]:checked').each(function () {
        const amount = parseFloat($(this).data('amount'));
        if (!isNaN(amount)) total += amount;
    });
    return total;
}
