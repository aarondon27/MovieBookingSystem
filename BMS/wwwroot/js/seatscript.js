document.addEventListener("DOMContentLoaded", () => {

    const seats = document.querySelectorAll('.seat');
    const confirmBtn = document.getElementById("confirmBtn");
    const movieid = document.getElementById("movieid")
    function updatePayButton() {
        const selectedSeats = [...document.querySelectorAll('.seat.selected')];

        if (selectedSeats.length === 0) {
            confirmBtn.textContent = "Pay";
            return;
        }

        const total = selectedSeats
            .map(s => parseInt(s.getAttribute("data-price")))
            .reduce((a, b) => a + b, 0);

        confirmBtn.textContent = `Pay ₹${total}`;
    }

    seats.forEach(seat => {
        seat.addEventListener('click', () => {
            if (!seat.classList.contains("booked")) {
                seat.classList.toggle("selected");
                updatePayButton();
            }
        });
    });

    confirmBtn.addEventListener("click", () => {
        const selectedSeats = [...document.querySelectorAll('.seat.selected')];

        if (selectedSeats.length === 0) {
            alert("Please select at least one seat.");
            return;
        }

        const seatsList = selectedSeats.map(s => s.getAttribute("data-seat"));
        const total = confirmBtn.textContent.replace("Pay ₹", "").trim();

        window.location.href = `/Movies/Payment?seats=${encodeURIComponent(seatsList.join(","))}&total=${total}`;
    });
});

