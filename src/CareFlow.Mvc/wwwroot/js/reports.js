/**
 * CareFlow Admin Reports — jQuery AJAX client.
 * Calls the CareFlow Web API directly for dynamic report filtering.
 * MVC does not duplicate business logic; all data comes from http://localhost:5000.
 */
(function ($) {
    'use strict';

    var config = {};
    var currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' });

    function init() {
        var $cfg = $('#reports-config');
        if ($cfg.length === 0) return;

        config = {
            apiBaseUrl: $cfg.data('api-base-url'),
            apiToken: $cfg.data('api-token'),
            reportType: $cfg.data('report-type')
        };

        $('#btn-refresh-report').on('click', refreshReport);
        $('#report-filters select, #report-filters input').on('change', refreshReport);
    }

    function setStatus(text, cssClass) {
        var $badge = $('#refresh-indicator');
        if ($badge.length) {
            $badge.text(text).removeClass('bg-secondary bg-primary bg-success bg-danger').addClass(cssClass || 'bg-secondary');
        }
    }

    function showError(message) {
        var $alert = $('#report-alert');
        $alert.text(message).removeClass('d-none');
    }

    function hideError() {
        $('#report-alert').addClass('d-none');
    }

    function apiGet(path, params) {
        return $.ajax({
            url: config.apiBaseUrl + path,
            method: 'GET',
            headers: { Authorization: 'Bearer ' + config.apiToken },
            data: params || {}
        });
    }

    function refreshReport() {
        hideError();
        setStatus('Loading...', 'bg-primary');

        if (config.reportType === 'dashboard') {
            loadDashboard();
        } else if (config.reportType === 'revenue') {
            loadRevenue();
        } else if (config.reportType === 'appointments') {
            loadAppointments();
        }
    }

    function loadDashboard() {
        apiGet('/api/dashboard/admin')
            .done(function (response) {
                if (!response.success || !response.data) {
                    showError(response.message || 'Failed to load dashboard.');
                    setStatus('Error', 'bg-danger');
                    return;
                }
                updateDashboardStats(response.data);
                updateDepartmentTable(response.data.appointmentsByDepartment || []);
                updateDoctorTable(response.data.appointmentsByDoctor || []);
                setStatus('Updated', 'bg-success');
            })
            .fail(function (xhr) {
                showError('API error: ' + (xhr.responseJSON?.message || xhr.statusText));
                setStatus('Error', 'bg-danger');
            });
    }

    function updateDashboardStats(data) {
        var stats = [
            { label: 'Total Patients', value: data.totalPatients },
            { label: 'Total Doctors', value: data.totalDoctors },
            { label: "Today's Appointments", value: data.todayAppointments },
            { label: 'Pending Confirmations', value: data.pendingConfirmations },
            { label: 'Monthly Revenue', value: currency.format(data.monthlyRevenue) },
            { label: 'Pending Payments', value: currency.format(data.pendingPayments) }
        ];

        $('#dashboard-stats .stat-value').each(function (i) {
            if (stats[i]) {
                $(this).text(typeof stats[i].value === 'number' && i >= 4 ? stats[i].value : stats[i].value);
            }
        });
    }

    function updateDepartmentTable(items) {
        var $tbody = $('#dept-table tbody').empty();
        items.forEach(function (item) {
            $tbody.append('<tr><td>' + escapeHtml(item.department) + '</td><td class="text-end">' + item.count + '</td></tr>');
        });
    }

    function updateDoctorTable(items) {
        var $tbody = $('#doctor-table tbody').empty();
        items.forEach(function (item) {
            $tbody.append('<tr><td>' + escapeHtml(item.doctorName) + '</td><td class="text-end">' + item.count + '</td></tr>');
        });
    }

    function loadRevenue() {
        var paymentStatus = $('#paymentStatus').val();
        var params = { pageNumber: 1, pageSize: 100 };
        if (paymentStatus) params.paymentStatus = paymentStatus;

        apiGet('/api/invoices', params)
            .done(function (response) {
                if (!response.success || !response.data) {
                    showError(response.message || 'Failed to load invoices.');
                    setStatus('Error', 'bg-danger');
                    return;
                }
                renderRevenueTable(response.data.items || []);
                setStatus('Updated', 'bg-success');
            })
            .fail(function (xhr) {
                showError('API error: ' + (xhr.responseJSON?.message || xhr.statusText));
                setStatus('Error', 'bg-danger');
            });
    }

    function renderRevenueTable(invoices) {
        var $tbody = $('#revenue-table tbody').empty();
        var totalRevenue = 0;
        var pendingAmount = 0;

        invoices.forEach(function (inv) {
            if (inv.paymentStatus === 'Paid' || inv.paymentStatus === 'PartiallyPaid') {
                totalRevenue += inv.totalAmount;
            }
            if (inv.paymentStatus === 'Pending') {
                pendingAmount += inv.totalAmount;
            }

            $tbody.append(
                '<tr>' +
                '<td>' + escapeHtml(inv.invoiceNumber) + '</td>' +
                '<td>' + escapeHtml(inv.patientName) + '</td>' +
                '<td>' + formatDate(inv.invoiceDate) + '</td>' +
                '<td class="text-end">' + currency.format(inv.subTotal) + '</td>' +
                '<td class="text-end">' + currency.format(inv.taxAmount) + '</td>' +
                '<td class="text-end">' + currency.format(inv.discountAmount) + '</td>' +
                '<td class="text-end">' + currency.format(inv.totalAmount) + '</td>' +
                '<td><span class="badge bg-secondary">' + escapeHtml(inv.paymentStatus) + '</span></td>' +
                '</tr>'
            );
        });

        $('#dashboard-stats .stat-value, .card .stat-value').each(function () {
            /* stat cards on revenue page */
        });

        var $statCards = $('#dashboard-stats .stat-value');
        if ($statCards.length >= 3) {
            $statCards.eq(0).text(currency.format(totalRevenue));
            $statCards.eq(1).text(currency.format(pendingAmount));
            $statCards.eq(2).text(invoices.length);
        }
    }

    function loadAppointments() {
        var status = $('#status').val();
        var doctorId = $('#doctorId').val();
        var params = { pageNumber: 1, pageSize: 100 };
        if (status) params.status = status;
        if (doctorId) params.doctorId = doctorId;

        apiGet('/api/appointments', params)
            .done(function (response) {
                if (!response.success || !response.data) {
                    showError(response.message || 'Failed to load appointments.');
                    setStatus('Error', 'bg-danger');
                    return;
                }
                renderAppointmentsTable(response.data.items || []);
                setStatus('Updated', 'bg-success');
            })
            .fail(function (xhr) {
                showError('API error: ' + (xhr.responseJSON?.message || xhr.statusText));
                setStatus('Error', 'bg-danger');
            });
    }

    function renderAppointmentsTable(appointments) {
        var $tbody = $('#appointments-table tbody').empty();
        appointments.forEach(function (appt) {
            var start = formatTime(appt.startTime);
            var end = formatTime(appt.endTime);
            $tbody.append(
                '<tr>' +
                '<td>' + escapeHtml(appt.patientName) + '<br><small class="text-muted">' + escapeHtml(appt.patientNumber) + '</small></td>' +
                '<td>' + escapeHtml(appt.doctorName) + '</td>' +
                '<td>' + escapeHtml(appt.departmentName) + '</td>' +
                '<td>' + formatDate(appt.appointmentDate) + '</td>' +
                '<td>' + start + ' - ' + end + '</td>' +
                '<td><span class="badge bg-secondary">' + escapeHtml(appt.status) + '</span></td>' +
                '<td>' + escapeHtml(appt.reason || '') + '</td>' +
                '</tr>'
            );
        });

        $('.card-title').filter(function () { return $(this).text().indexOf('Appointments') === 0; })
            .first().text('Appointments (' + appointments.length + ')');
    }

    function formatDate(dateStr) {
        return new Date(dateStr).toLocaleDateString();
    }

    function formatTime(timeStr) {
        if (typeof timeStr === 'string' && timeStr.indexOf(':') >= 0) {
            var parts = timeStr.split(':');
            return parts[0] + ':' + parts[1];
        }
        return timeStr;
    }

    function escapeHtml(text) {
        if (!text) return '';
        return $('<div>').text(text).html();
    }

    $(document).ready(init);
})(jQuery);
