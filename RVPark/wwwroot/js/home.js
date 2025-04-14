document.addEventListener("DOMContentLoaded", function () {
    const sortSelect = document.getElementById("sortOptions");
    const lotContainer = document.getElementById("lotContainer");

    if (sortSelect && lotContainer) {
        sortSelect.addEventListener("change", function () {
            const sort = this.value;
            const cards = Array.from(lotContainer.getElementsByClassName("lot-card"));

            cards.sort((a, b) => {
                const nameA = a.dataset.name.toLowerCase();
                const nameB = b.dataset.name.toLowerCase();
                const priceA = parseFloat(a.dataset.price);
                const priceB = parseFloat(b.dataset.price);

                switch (sort) {
                    case "az": return nameA.localeCompare(nameB);
                    case "za": return nameB.localeCompare(nameA);
                    case "high": return priceB - priceA;
                    case "low": return priceA - priceB;
                    default: return 0;
                }
            });

            cards.forEach(card => lotContainer.appendChild(card));
        });
    }
});

// Thumbnail image click → main image update
function setMainImage(url) {
    const mainImage = document.getElementById("mainImage");
    if (mainImage) {
        mainImage.src = url;
    }
}

// Vertical carousel scroll
function scrollThumbnails(direction) {
    const carousel = document.getElementById("thumbnailCarousel");
    if (!carousel) return;

    const scrollAmount = 100;
    carousel.scrollBy({ top: direction * scrollAmount, behavior: 'smooth' });
}

// Fullscreen preview
function openFullscreen() {
    const mainImage = document.getElementById("mainImage");
    if (!mainImage) return;

    const fullscreenContainer = document.createElement("div");
    fullscreenContainer.id = "fullscreenOverlay";
    fullscreenContainer.style.position = "fixed";
    fullscreenContainer.style.top = "0";
    fullscreenContainer.style.left = "0";
    fullscreenContainer.style.width = "100vw";
    fullscreenContainer.style.height = "100vh";
    fullscreenContainer.style.background = "rgba(0, 0, 0, 0.9)";
    fullscreenContainer.style.display = "flex";
    fullscreenContainer.style.justifyContent = "center";
    fullscreenContainer.style.alignItems = "center";
    fullscreenContainer.style.zIndex = "9999";

    const fullImage = document.createElement("img");
    fullImage.src = mainImage.src;
    fullImage.style.maxWidth = "95vw";
    fullImage.style.maxHeight = "95vh";
    fullImage.style.border = "6px solid white";
    fullImage.style.borderRadius = "10px";
    fullImage.alt = "Fullscreen Preview";

    fullscreenContainer.appendChild(fullImage);

    fullscreenContainer.addEventListener("click", () => {
        fullscreenContainer.remove();
        document.body.style.overflow = "";
    });

    document.body.appendChild(fullscreenContainer);
    document.body.style.overflow = "hidden";
}
