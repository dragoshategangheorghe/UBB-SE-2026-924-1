$(function () {

    const TAB_PERSONAL = 'personal';
    const TAB_SECURITY = 'security';
    const TAB_NOTIFICATIONS = 'notifications';
    const MIN_PASSWORD_LEN = 8;

    let editUnlocked = false;


    function activateTab(tabName) {
        // Panel visibility
        $('#panel-personal, #panel-security, #panel-notifications').hide();
        $(`#panel-${tabName}`).show();

        // Tab button styles
        $('.profile-tab-btn').removeClass('active');
        $(`.profile-tab-btn[data-tab="${tabName}"]`).addClass('active');

        // Keep URL in sync so a redirect back lands on the right tab
        const url = new URL(window.location.href);
        url.searchParams.set('tab', tabName);
        window.history.replaceState(null, '', url.toString());
    }

    $('.profile-tab-btn').on('click', function () {
        activateTab($(this).data('tab'));
    });

    // Activate the tab the server told us to show (after a redirect)
    activateTab($('#active-tab-input').val() || TAB_PERSONAL);


    setTimeout(function () {
        $('.alert-autodismiss').fadeOut(400, function () { $(this).remove(); });
    }, 5000);


    // "Unlock Update" opens the verify-password modal
    $('#unlock-edit-btn').on('click', function () {
        resetVerifyModal();
        $('#verify-modal-intent').val('edit');
        $('#verifyPasswordModal').modal('show');
    });

    // Confirm inside verify modal
    $('#verify-confirm-btn').on('click', function () {
        const pwd = $('#verify-password-input').val().trim();

        if (!pwd) {
            showModalError('#verify-modal-error', 'Please enter your password.');
            return;
        }

        // Submit the hidden verify form — the server validates and redirects back
        // with the tab pre-set.  If the intent is 'edit' we stay on personal tab
        // and the URL carries ?editUnlocked=1 so we re-enable the fields below.
        $('#verify-password-field').val(pwd);
        $('#verify-intent-field').val($('#verify-modal-intent').val());
        $('#verify-password-form').submit();
    });


    // Re-open in edit mode if server redirected back after successful verify
    const params = new URLSearchParams(window.location.search);
    if (params.get('editUnlocked') === '1') {
        setEditingEnabled(true);
        // Clean the URL param so F5 doesn't re-enable
        params.delete('editUnlocked');
        const cleanUrl = `${window.location.pathname}${params.toString() ? '?' + params.toString() : ''}`;
        window.history.replaceState(null, '', cleanUrl);
    }

    function setEditingEnabled(enabled) {
        editUnlocked = enabled;

        $('#phone-input, #address-input')
            .prop('readonly', !enabled)
            .toggleClass('input-unlocked', enabled);

        $('#save-changes-btn').prop('disabled', !enabled);
        $('#unlock-edit-btn').toggleClass('d-none', enabled);
        $('#save-changes-btn').toggleClass('d-none', !enabled);
    }


    $('#change-password-btn').on('click', function () {
        resetChangePasswordModal();
        $('#changePasswordModal').modal('show');
    });

    $('#change-password-confirm-btn').on('click', function () {
        const current = $('#cp-current-input').val();
        const newPwd = $('#cp-new-input').val();
        const confirm = $('#cp-confirm-input').val();

        if (!current) {
            showModalError('#cp-modal-error', 'Enter your current password.');
            return;
        }
        if (newPwd.length < MIN_PASSWORD_LEN) {
            showModalError('#cp-modal-error', `New password must be at least ${MIN_PASSWORD_LEN} characters.`);
            return;
        }
        if (newPwd !== confirm) {
            showModalError('#cp-modal-error', 'Passwords do not match.');
            return;
        }

        $('#cp-current-field').val(current);
        $('#cp-new-field').val(newPwd);
        $('#cp-confirm-field').val(confirm);
        $('#change-password-form').submit();
    });


    $('#twofa-toggle').on('change', function () {
        const enable = $(this).is(':checked');

        if (!enable) {
            // Disabling — confirm before submitting
            if (!confirm('Are you sure you want to disable two-factor authentication?')) {
                // Revert the toggle visually
                $(this).prop('checked', true);
                return;
            }
        }

        $('#twofa-enable-field').val(enable ? 'true' : 'false');
        $('#twofa-form').submit();
    });


    // Each toggle carries data-pref-id, data-channel attributes.
    // On change we update the corresponding hidden inputs and submit.
    $('#notifications-form').on('change', '.notif-toggle', function () {
        const prefId = $(this).data('pref-id');
        const channel = $(this).data('channel');   // email | sms | push
        const checked = $(this).is(':checked');

        // Update the matching hidden input
        $(`#notif-${prefId}-${channel}`).val(checked ? 'true' : 'false');

        $('#notifications-form').submit();
    });


    function showModalError(selector, message) {
        $(selector).text(message).removeClass('d-none');
    }

    function resetVerifyModal() {
        $('#verify-password-input').val('');
        $('#verify-modal-error').addClass('d-none').text('');
    }

    function resetChangePasswordModal() {
        $('#cp-current-input, #cp-new-input, #cp-confirm-input').val('');
        $('#cp-modal-error').addClass('d-none').text('');
    }

});