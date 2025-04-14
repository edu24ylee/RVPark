$(document).ready(function () {
    $('#Reservation_StartDate, #Reservation_EndDate').on('change', updateLotsAndCosts);
    $('#Reservation_LotTypeId, #Rv_Length').on('change', updateLotsAndCosts);
    $('#Reservation_LotId').on('change', updateCostsOnly);

    if (!$('#Reservation_LotTypeId').val()) {
        $('#Reservation_LotId').prop('disabled', true);
    }

    updateCostsOnly();
});

function updateLotsAndCosts() {
    const lotTypeId = $('#Reservation_LotTypeId').val();
    const trailerLength = $('#Rv_Length').val();
    const startDate = $('#Reservation_StartDate').val();
    const endDate = $('#Reservation_EndDate').val();

    if (!lotTypeId || !trailerLength || !startDate || !endDate) return;

    $('#Reservation_LotId').prop('disabled', false);

    $.get('/Admin/Reservations/Update?handler=AvailableLots', {
        lotTypeId,
        trailerLength,
        startDate,
        endDate
    }, function (lots) {
        const $lotSelect = $('#Reservation_LotId');
        $lotSelect.empty();

        if (!lots.length) {
            $lotSelect.append(`<option disabled>No lots available</option>`);
            updateCostsOnly();
            return;
        }

        lots.forEach(lot => {
            $lotSelect.append(`<option value="${lot.id}" data-rate="${lot.lotTypeRate}">
                Lot #${lot.id} - ${lot.location}</option>`);
        });

        updateCostsOnly();
    });
}

function updateCostsOnly() {
    const startDateStr = $('#Reservation_StartDate').val();
    const endDateStr = $('#Reservation_EndDate').val();
    const selectedRate = parseFloat($('#Reservation_LotId option:selected').data('rate')) || 0;
    const originalTotal = parseFloat($('#originalTotal').val()) || 0;

    if (!startDateStr || !endDateStr || !selectedRate) {
        $('#Reservation_Duration').val('');
        $('#updatedTotalField').val('$0.00');
        $('#BalanceDifferenceDisplay').val('$0.00');
        return;
    }

    const start = new Date(startDateStr);
    const end = new Date(endDateStr);
    const duration = Math.max(0, Math.ceil((end - start) / (1000 * 60 * 60 * 24)));

    let updatedTotal = duration * selectedRate;
    const extraAdultFee = calculateExtraAdultFee();
    const cancellationFee = calculateCancellationFee();

    updatedTotal += extraAdultFee + cancellationFee;
    const balanceDiff = updatedTotal - originalTotal;

    $('#Reservation_Duration').val(duration);
    $('#updatedTotalField').val(`$${updatedTotal.toFixed(2)}`);
    $('#BalanceDifferenceDisplay').val(`$${balanceDiff.toFixed(2)}`);
}

function calculateExtraAdultFee() {
    const adults = parseInt($('#NumberOfAdults').val()) || 0;
    const perExtraFee = 10;
    return adults > 2 ? (adults - 2) * perExtraFee : 0;
}

function calculateCancellationFee() {
    return 0; // Can be implemented if needed
}
