// ========================================
// Site-wide Utilities for Docs & Plannings
// ========================================

// Namespace to avoid global pollution
const DocsAndPlannings = (function () {
    'use strict';

    // ========================================
    // Flash Message System
    // ========================================
    const FlashMessage = {
        /**
         * Show a flash message
         * @param {string} message - The message to display
         * @param {string} type - Message type: success, error, warning, info
         * @param {number} duration - Duration in milliseconds (0 for no auto-hide)
         */
        show: function (message, type = 'info', duration = 5000) {
            const container = document.getElementById('flashMessageContainer');
            if (!container) {
                console.error('Flash message container not found');
                return;
            }

            const alertClass = this._getAlertClass(type);
            const iconClass = this._getIconClass(type);

            const alert = document.createElement('div');
            alert.className = `alert ${alertClass} alert-dismissible fade show flash-message`;
            alert.setAttribute('role', 'alert');
            alert.innerHTML = `
                <i class="${iconClass} me-2"></i>
                <span>${this._escapeHtml(message)}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            `;

            container.appendChild(alert);

            // Auto-hide after duration
            if (duration > 0) {
                setTimeout(() => {
                    alert.classList.add('fade-out');
                    setTimeout(() => {
                        alert.remove();
                    }, 300);
                }, duration);
            }
        },

        _getAlertClass: function (type) {
            const alertClasses = {
                success: 'alert-success',
                error: 'alert-danger',
                warning: 'alert-warning',
                info: 'alert-info'
            };
            return alertClasses[type] || 'alert-info';
        },

        _getIconClass: function (type) {
            const iconClasses = {
                success: 'bi bi-check-circle-fill',
                error: 'bi bi-exclamation-triangle-fill',
                warning: 'bi bi-exclamation-circle-fill',
                info: 'bi bi-info-circle-fill'
            };
            return iconClasses[type] || 'bi bi-info-circle-fill';
        },

        _escapeHtml: function (text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, m => map[m]);
        }
    };

    // ========================================
    // Loading Spinner
    // ========================================
    const LoadingSpinner = {
        overlay: null,

        /**
         * Show loading spinner overlay
         * @param {string} message - Optional loading message
         */
        show: function (message = 'Loading...') {
            if (this.overlay) {
                return; // Already showing
            }

            this.overlay = document.createElement('div');
            this.overlay.className = 'spinner-overlay';
            this.overlay.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border spinner-border-lg text-light" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-light mt-3">${this._escapeHtml(message)}</div>
                </div>
            `;

            document.body.appendChild(this.overlay);
        },

        /**
         * Hide loading spinner overlay
         */
        hide: function () {
            if (this.overlay) {
                this.overlay.remove();
                this.overlay = null;
            }
        },

        _escapeHtml: function (text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, m => map[m]);
        }
    };

    // ========================================
    // Form Utilities
    // ========================================
    const FormUtils = {
        /**
         * Add loading state to a button
         * @param {HTMLElement} button - The button element
         * @param {string} loadingText - Text to show while loading
         */
        setButtonLoading: function (button, loadingText = 'Loading...') {
            if (!button) return;

            button.disabled = true;
            button.dataset.originalText = button.innerHTML;
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                ${loadingText}
            `;
        },

        /**
         * Remove loading state from a button
         * @param {HTMLElement} button - The button element
         */
        resetButton: function (button) {
            if (!button) return;

            button.disabled = false;
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
                delete button.dataset.originalText;
            }
        },

        /**
         * Serialize form data to JSON
         * @param {HTMLFormElement} form - The form element
         * @returns {Object} Form data as JSON object
         */
        serializeToJson: function (form) {
            const formData = new FormData(form);
            const json = {};

            for (const [key, value] of formData.entries()) {
                if (json[key]) {
                    if (Array.isArray(json[key])) {
                        json[key].push(value);
                    } else {
                        json[key] = [json[key], value];
                    }
                } else {
                    json[key] = value;
                }
            }

            return json;
        },

        /**
         * Validate form before submission
         * @param {HTMLFormElement} form - The form element
         * @returns {boolean} True if valid
         */
        validateForm: function (form) {
            if (!form) return false;

            // Use HTML5 validation
            if (typeof form.checkValidity === 'function') {
                if (!form.checkValidity()) {
                    form.classList.add('was-validated');
                    return false;
                }
            }

            return true;
        }
    };

    // ========================================
    // CSRF Token Helper
    // ========================================
    const CsrfToken = {
        /**
         * Get CSRF token from the page
         * @returns {string|null} CSRF token value
         */
        get: function () {
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            return token ? token.value : null;
        },

        /**
         * Get CSRF token name
         * @returns {string} Token field name
         */
        getFieldName: function () {
            return '__RequestVerificationToken';
        }
    };

    // ========================================
    // Utility Functions
    // ========================================
    const Utils = {
        /**
         * Debounce function execution
         * @param {Function} func - Function to debounce
         * @param {number} wait - Wait time in milliseconds
         * @returns {Function} Debounced function
         */
        debounce: function (func, wait = 300) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },

        /**
         * Format date to relative time (e.g., "2 hours ago")
         * @param {Date|string} date - Date to format
         * @returns {string} Formatted relative time
         */
        formatRelativeTime: function (date) {
            const dateObj = typeof date === 'string' ? new Date(date) : date;
            const seconds = Math.floor((new Date() - dateObj) / 1000);

            const intervals = {
                year: 31536000,
                month: 2592000,
                week: 604800,
                day: 86400,
                hour: 3600,
                minute: 60
            };

            for (const [unit, secondsInUnit] of Object.entries(intervals)) {
                const interval = Math.floor(seconds / secondsInUnit);
                if (interval >= 1) {
                    return interval === 1 ? `1 ${unit} ago` : `${interval} ${unit}s ago`;
                }
            }

            return 'just now';
        },

        /**
         * Copy text to clipboard
         * @param {string} text - Text to copy
         * @returns {Promise<void>}
         */
        copyToClipboard: async function (text) {
            try {
                await navigator.clipboard.writeText(text);
                FlashMessage.show('Copied to clipboard!', 'success', 2000);
            } catch (err) {
                console.error('Failed to copy text:', err);
                FlashMessage.show('Failed to copy to clipboard', 'error', 3000);
            }
        }
    };

    // ========================================
    // Initialize on DOM ready
    // ========================================
    function init() {
        // Check for TempData flash messages
        const flashData = document.getElementById('tempDataFlashMessage');
        if (flashData) {
            const message = flashData.dataset.message;
            const type = flashData.dataset.type || 'info';
            if (message) {
                FlashMessage.show(message, type);
            }
        }

        // Add confirmation dialogs to elements with data-confirm attribute
        document.addEventListener('click', function (e) {
            const target = e.target.closest('[data-confirm]');
            if (target) {
                const message = target.dataset.confirm || 'Are you sure?';
                if (!confirm(message)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            }
        });

        // Auto-dismiss alerts after 5 seconds
        document.querySelectorAll('.alert:not(.flash-message)').forEach(alert => {
            setTimeout(() => {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                bsAlert.close();
            }, 5000);
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // ========================================
    // Public API
    // ========================================
    return {
        FlashMessage: FlashMessage,
        LoadingSpinner: LoadingSpinner,
        FormUtils: FormUtils,
        CsrfToken: CsrfToken,
        Utils: Utils
    };
})();

// Make available globally for inline scripts if needed
window.DocsAndPlannings = DocsAndPlannings;
