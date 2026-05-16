
    document.addEventListener('DOMContentLoaded', function () {
        var myCarousel = document.getElementById('cardCarousel');
        var dots = document.querySelectorAll('.dot');

        myCarousel.addEventListener('slide.bs.carousel', function (e) {
            dots.forEach(dot => dot.classList.remove('active'));
            dots[e.to].classList.add('active');
        });
    });

    function showCardDetails(type, brand, number, holder, expiry, status, contactless, online) {
        const infoContent =
            `Card Type: ${type}\n` +
            `Card Brand: ${brand}\n` +
            `Card Number: ${number}\n` +
            `Cardholder: ${holder}\n` +
            `Expiry Date: ${expiry}\n` +
            `Status: ${status}\n` +
            `Contactless: ${contactless}\n` +
            `Online Payments: ${online}`;
        document.getElementById('modalCardInfo').textContent = infoContent;

        var myModal = new bootstrap.Modal(document.getElementById('cardDetailsModal'));
        myModal.show();
    }

