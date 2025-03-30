function calculateUpdatedTotal() {
    const startInput = document.querySelector('[name="Reservation.StartDate"]');
    const endInput = document.querySelector('[name="Reservation.EndDate"]');
    const lotSelect = document.querySelector('[name="Reservation.LotId"]');

    const start = new Date(startInput.value);
    const end = new Date(endInput.value);

    const durationField = document.querySelector('[name="Reservation.Duration"]');
    const updatedTotalField = document.getElementById("updatedTotalField");
    const balanceField = document.getElementById("balanceDifferenceField");
    const originalTotal = parseFloat(document.getElementById("originalTotal").value);

    if (isNaN(start.getTime()) || isNaN(end.getTime()) || end <= start) {
        durationField.value = "";
        updatedTotalField.value = "";
        balanceField.value = "";
        return;
    }

    const duration = Math.floor((end - start) / (1000 * 60 * 60 * 24));
    durationField.value = duration;

    const selectedOption = lotSelect.options[lotSelect.selectedIndex];
    const rateAttr = selectedOption.getAttribute("data-rate");

    if (!rateAttr) return;

    const rate = parseFloat(rateAttr);
    const newTotal = duration * rate;
    const diff = newTotal - originalTotal;

    updatedTotalField.value = newTotal.toFixed(2);
    balanceField.value = (diff >= 0 ? "+" : "") + diff.toFixed(2);
}
