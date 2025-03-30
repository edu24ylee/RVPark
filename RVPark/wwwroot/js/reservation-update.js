function calculateUpdatedTotal() {
    const startInput = document.querySelector('[name="ViewModel.Reservation.StartDate"]');
    const endInput = document.querySelector('[name="ViewModel.Reservation.EndDate"]');
    const lotSelect = document.querySelector('[name="ViewModel.Reservation.LotId"]');
    const updatedTotalField = document.getElementById("updatedTotalField");

    const start = new Date(startInput.value);
    const end = new Date(endInput.value);

    if (isNaN(start.getTime()) || isNaN(end.getTime()) || end <= start) {
        updatedTotalField.value = "";
        return;
    }

    const duration = Math.floor((end - start) / (1000 * 60 * 60 * 24));
    document.querySelector('[name="ViewModel.Reservation.Duration"]').value = duration;

    const selectedOption = lotSelect.options[lotSelect.selectedIndex];
    const rate = parseFloat(selectedOption.getAttribute("data-rate"));

    if (isNaN(rate)) {
        updatedTotalField.value = "";
        return;
    }

    const newTotal = duration * rate;
    updatedTotalField.value = newTotal.toFixed(2);
}

window.addEventListener("DOMContentLoaded", function () {
    calculateUpdatedTotal();
});

document.addEventListener("input", function (e) {
    const names = [
        "ViewModel.Reservation.StartDate",
        "ViewModel.Reservation.EndDate",
        "ViewModel.Reservation.LotId"
    ];
    if (names.includes(e.target.name)) {
        calculateUpdatedTotal();
    }
});
