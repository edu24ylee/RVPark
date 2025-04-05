// This file manages real-time updates to the reservation update form.
// It changes available lots based on trailer length, recalculates cost differences,
// and updates the UI responsively as inputs change.

$(document).ready(function () {
    // Attach event handlers as soon as the document is ready
    $('#Reservation_StartDate, #Reservation_EndDate, #Reservation_LotId').on('change', updateCost);
    $('#Reservation_LotTypeId').on('change', updateAvailableLots);
    $('#Rv_Length').on('change', updateAvailableLots);

    // Trigger both updates once on load
    updateAvailableLots();
    updateCost();
});

function updateAvailableLots() {
    const lotTypeId = $('#Reservation_LotTypeId').val();   // Selected lot type
    const trailerLength = $('#Rv_Length').val();           // Selected RV/trailer length
    const startDate = $('#Reservation_StartDate').val();   // Selected start date
    const endDate = $('#Reservation_EndDate').val();       // Selected end date

    if (!lotTypeId || !startDate || !endDate || !trailerLength) {
        return; // Exit if any required values are missing
    }

    // Request a list of available lots for the specified criteria
    $.get(`/Admin/Reservations/GetAvailableLots`, {
        lotTypeId,
        trailerLength,
        startDate,
        endDate
    }, function (lots) {
        const $lotSelect = $('#Reservation_LotId');
        $lotSelect.empty(); // Clear existing options

        if (lots.length === 0) {
            $lotSelect.append(`<option disabled>No lots available</option>`);
            return;
        }

        // Populate dropdown with available lot options
        lots.forEach(lot => {
            $lotSelect.append(`<option value="${lot.id}">${lot.location}</option>`);
        });

        // Trigger a cost update if lot changes
        updateCost();
    });
}

function updateCost() {
    const lotId = $('#Reservation_LotId').val();
    const startDate = $('#Reservation_StartDate').val();
    const endDate = $('#Reservation_EndDate').val();

    if (!lotId || !startDate || !endDate) {
        $('#BalanceDifferenceDisplay').text('$0.00');
        return;
    }

    // Call backend to calculate price difference between original and new values
    $.get('/Admin/Reservations/CalculateBalanceDifference', {
        lotId,
        startDate,
        endDate
    }, function (response) {
        // Update display with formatted difference
        $('#BalanceDifferenceDisplay').text(`$${parseFloat(response).toFixed(2)}`);
    });
}
