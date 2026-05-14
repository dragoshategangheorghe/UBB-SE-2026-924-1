(function ($) {
    function debounce(fn, delay) {
        var timerId = null;
        return function () {
            var context = this;
            var args = arguments;
            window.clearTimeout(timerId);
            timerId = window.setTimeout(function () {
                fn.apply(context, args);
            }, delay);
        };
    }

    function redirectToAuthIfNeeded(xhr) {
        if (xhr && xhr.status === 401) {
            window.location.href = "/Auth";
        }
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat(undefined, {
            style: "currency",
            currency: "USD"
        }).format(value || 0);
    }

    $(function () {
        var savingsTypeSelect = $(".js-savings-type");

        function syncSavingsTypePanels() {
            var selectedType = savingsTypeSelect.val();
            $(".goal-settings").toggleClass("d-none", selectedType !== "GoalSavings");
            $(".fixed-settings").toggleClass("d-none", selectedType !== "FixedDeposit");
        }

        savingsTypeSelect.on("change", syncSavingsTypePanels);
        syncSavingsTypePanels();

        $(".js-deposit-preview-form").each(function () {
            var form = $(this);
            var amountInput = form.find(".js-deposit-amount");
            var previewLabel = form.find(".js-deposit-preview");
            var accountId = form.find("input[name='Deposit.AccountId']").val();
            var previewUrl = form.data("preview-url");

            var updatePreview = debounce(function () {
                $.get(previewUrl, {
                    accountId: accountId,
                    amountText: amountInput.val()
                }).done(function (data) {
                    previewLabel.text(data.preview || "");
                }).fail(redirectToAuthIfNeeded);
            }, 200);

            amountInput.on("input", updatePreview);
        });

        $(".js-withdraw-preview-form").each(function () {
            var form = $(this);
            var amountInput = form.find(".js-withdraw-amount");
            var breakdown = form.find(".js-withdraw-breakdown");
            var netAmount = form.find(".js-withdraw-net");
            var accountId = form.find("input[name='Withdraw.AccountId']").val();
            var previewUrl = form.data("preview-url");

            var updatePreview = debounce(function () {
                $.get(previewUrl, {
                    accountId: accountId,
                    amountText: amountInput.val()
                }).done(function (data) {
                    if (data.penaltyBreakdown) {
                        breakdown.text(data.penaltyBreakdown).removeClass("d-none");
                    } else {
                        breakdown.addClass("d-none");
                    }

                    if (data.netAmountText) {
                        netAmount.text(data.netAmountText).removeClass("d-none");
                    } else {
                        netAmount.addClass("d-none");
                    }
                }).fail(redirectToAuthIfNeeded);
            }, 200);

            amountInput.on("input", updatePreview);
        });

        $(".js-loan-estimate-form").each(function () {
            var form = $(this);
            var estimateCard = form.find(".estimate-card");
            var estimateUrl = form.data("estimate-url");

            var updateEstimate = debounce(function () {
                $.get(estimateUrl, {
                    loanType: form.find(".js-loan-type").val(),
                    desiredAmount: form.find(".js-loan-amount").val(),
                    preferredTermMonths: form.find(".js-loan-term").val(),
                    purpose: form.find(".js-loan-purpose").val()
                }).done(function (data) {
                    if (!data.show) {
                        estimateCard.addClass("d-none");
                        return;
                    }

                    estimateCard.removeClass("d-none");
                    form.find(".js-estimate-rate").text(data.rate);
                    form.find(".js-estimate-monthly").text(data.monthly);
                    form.find(".js-estimate-total").text(data.total);
                }).fail(function (xhr) {
                    estimateCard.addClass("d-none");
                    redirectToAuthIfNeeded(xhr);
                });
            }, 220);

            form.on("change input", ".js-loan-type, .js-loan-amount, .js-loan-term, .js-loan-purpose", updateEstimate);
        });

        var paymentModal = $("#paymentModal");
        var paymentForm = paymentModal.find(".js-payment-form");
        var customPaymentWrap = paymentForm.find(".custom-payment-wrap");
        var previewError = paymentForm.find(".js-preview-error");

        function toggleCustomPayment() {
            var isCustom = paymentForm.find(".js-payment-mode:checked").val() === "true";
            customPaymentWrap.toggleClass("d-none", !isCustom);
        }

        function refreshPaymentPreview() {
            var previewUrl = paymentForm.data("preview-url");
            var useCustomAmount = paymentForm.find(".js-payment-mode:checked").val() === "true";

            $.get(previewUrl, {
                loanId: paymentForm.find(".js-payment-loan-id").val(),
                useCustomAmount: useCustomAmount,
                customAmount: paymentForm.find(".js-payment-custom-amount").val()
            }).done(function (data) {
                if (data.errorMessage) {
                    previewError.text(data.errorMessage).removeClass("d-none");
                    return;
                }

                previewError.addClass("d-none").text("");
                paymentForm.find(".js-preview-balance").text(formatCurrency(data.balanceAfterPayment));
                paymentForm.find(".js-preview-remaining").text((data.remainingMonthsAfterPayment || 0) + " mo");
            }).fail(function (xhr) {
                redirectToAuthIfNeeded(xhr);
            });
        }

        paymentModal.on("show.bs.modal", function (event) {
            var button = $(event.relatedTarget);
            paymentForm.find(".js-payment-loan-id").val(button.data("loan-id"));
            paymentForm.find(".js-payment-balance").text(formatCurrency(parseFloat(button.data("balance"))));
            paymentForm.find(".js-payment-installment").text(formatCurrency(parseFloat(button.data("installment"))));
            paymentForm.find(".js-payment-mode[value='false']").prop("checked", true);
            paymentForm.find(".js-payment-custom-amount").val("");
            toggleCustomPayment();
            refreshPaymentPreview();
        });

        paymentForm.on("change", ".js-payment-mode", function () {
            toggleCustomPayment();
            refreshPaymentPreview();
        });

        paymentForm.on("input", ".js-payment-custom-amount", debounce(refreshPaymentPreview, 200));
    });
})(jQuery);
