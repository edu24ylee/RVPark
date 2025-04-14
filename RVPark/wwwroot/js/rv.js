<script>
    function toggleCustomLength(select) {
        const container = document.getElementById("customLengthContainer");
    const input = document.getElementById("customLengthInput");

    if (select.value === "custom") {
        container.style.display = "block";
    input.value = ""; 
        } else {
        container.style.display = "none";
    input.value = select.value;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        const input = document.getElementById("customLengthInput");
    const dropdown = document.getElementById("lengthDropdown");

    if (input && dropdown) {
            const val = input.value?.trim();
    if (["20", "25", "30", "35", "40"].includes(val)) {
        dropdown.value = val;
    document.getElementById("customLengthContainer").style.display = "none";
            } else if (val) {
        dropdown.value = "custom";
    document.getElementById("customLengthContainer").style.display = "block";
            }
        }
    });
</script>
