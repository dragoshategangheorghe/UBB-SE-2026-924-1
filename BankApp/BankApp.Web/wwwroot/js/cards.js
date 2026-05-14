(function ($) {
    let countdownTimer = null;

    function showStatus(message, isSuccess) {
        const box = $("#cardsStatusBox");
        box.removeClass("d-none alert-success alert-danger");
        box.addClass(isSuccess ? "alert-success" : "alert-danger");
        box.text(message);
    }

    function getSelectedCardId() {
        return parseInt($("#selectedCardId").val(), 10);
    }

    function hideSensitiveDetails() {
        $("#sensitiveDetailsPanel").addClass("d-none");
        $("#revealedCardNumber").text("");
        $("#revealedCvv").text("");
        $("#revealCountdown").text("");

        if (countdownTimer) {
            clearInterval(countdownTimer);
            countdownTimer = null;
        }
    }

    function startCountdown(seconds) {
        let remaining = seconds;

        $("#revealCountdown").text(`Visible for ${remaining} more seconds.`);

        if (countdownTimer) {
            clearInterval(countdownTimer);
      
        }

        countdownTimer = setInterval(function () {
            remaining--;

            if (remaining <= 0) {
                hideSensitiveDetails();
                return;
            }

            $("#revealCountdown").text(`Visible for ${remaining} more seconds.`);
        }, 1000);
    }

    function selectCard(button) {
        $(".card-item").removeClass("active");
        button.addClass("active");

        $("#selectedCardId").val(button.data("card-id"));
        $("#selectedCardholder").text(button.data("cardholder"));
        $("#selectedMaskedNumber").text(button.data("masked"));
        $("#selectedAccountName").text(button.data("accountname"));
        $("#selectedAccountIban").text(button.data("accountiban"));
        $("#selectedStatus").text(button.data("status"));
        $("#selectedExpiry").text(button.data("expiry"));

        const limit = button.data("limit");
        $("#spendingLimit").val(limit ?? "");
        $("#onlinePayments").prop("checked", button.data("online") === true || button.data("online") === "true");
        $("#contactlessPayments").prop("checked", button.data("contactless") === true || button.data("contactless") === "true");

        $("#noCardPanel").addClass("d-none");
        $("#selectedCardPanel").removeClass("d-none");

        hideSensitiveDetails();
    }

    $(function () {
        $(".card-item").on("click", function () {
            selectCard($(this));
        });

        $("#applySortBtn").on("click", function () {
            $.ajax({
                url: "/Cards/UpdateSort",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    sortOption: $("#sortOption").val()
                })
            }).done(function (response) {
                showStatus(response.message, response.success);
                if (response.success) {
                    location.reload();
                }
            }).fail(function () {
                showStatus("Sort update failed.", false);
            });
        });

        $("#freezeBtn").on("click", function () {
            $.ajax({
                url: "/Cards/Freeze",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    cardId: getSelectedCardId()
                })
            }).done(function (response) {
                showStatus(response.message, response.success);
                if (response.success) {
                    location.reload();
                }
            }).fail(function () {
                showStatus("Freeze failed.", false);
            });
        });

        $("#unfreezeBtn").on("click", function () {
            $.ajax({
                url: "/Cards/Unfreeze",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    cardId: getSelectedCardId()
                })
            }).done(function (response) {
                showStatus(response.message, response.success);
                if (response.success) {
                    location.reload();
                }
            }).fail(function () {
                showStatus("Unfreeze failed.", false);
            });
        });

        $("#saveSettingsBtn").on("click", function () {
            const limitValue = $("#spendingLimit").val();

            $.ajax({
                url: "/Cards/UpdateSettings",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    cardId: getSelectedCardId(),
                    spendingLimit: limitValue === "" ? null : parseFloat(limitValue),
                    isOnlinePaymentsEnabled: $("#onlinePayments").is(":checked"),
                    isContactlessPaymentsEnabled: $("#contactlessPayments").is(":checked")
                })
            }).done(function (response) {
                showStatus(response.message, response.success);
                if (response.success) {
                    location.reload();
                }
            }).fail(function () {
                showStatus("Settings update failed.", false);
            });
        });

        $("#confirmRevealBtn").on("click", function () {
            $.ajax({
                url: "/Cards/Reveal",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    cardId: getSelectedCardId(),
                    password: $("#revealPassword").val(),
                    otpCode: $("#revealOtp").val()
                })
            }).done(function (response) {
                if (response.success && response.sensitiveDetails) {
                    $("#revealedCardNumber").text(response.sensitiveDetails.cardNumber);
                    $("#revealedCvv").text(response.sensitiveDetails.cvv);
                    $("#sensitiveDetailsPanel").removeClass("d-none");

                    showStatus(response.message, true);
                    startCountdown(response.revealDurationSeconds || 30);

                    const modalElement = document.getElementById("revealModal");
                    const modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) {
                        modal.hide();
                    }
                } else {
                    showStatus(response.message || "Reveal failed.", false);
                }
            }).fail(function () {
                showStatus("Reveal failed.", false);
            });
        });

        $("#hideSensitiveBtn").on("click", function () {
            hideSensitiveDetails();
        });
    });
})(jQuery);
