// ========================================
// API Client for Docs & Plannings
// Provides centralized API communication with CSRF protection
// ========================================

const ApiClient = (function () {
    'use strict';

    // Configuration
    const config = {
        baseUrl: '/api',
        defaultTimeout: 30000,
        defaultHeaders: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
    };

    // ========================================
    // Private Helper Functions
    // ========================================

    /**
     * Get CSRF token from the page
     * @returns {string|null}
     */
    function getCsrfToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : null;
    }

    /**
     * Build full URL from path
     * @param {string} path - API endpoint path
     * @returns {string}
     */
    function buildUrl(path) {
        if (path.startsWith('http://') || path.startsWith('https://')) {
            return path;
        }
        const cleanPath = path.startsWith('/') ? path : `/${path}`;
        return `${config.baseUrl}${cleanPath}`;
    }

    /**
     * Build request headers
     * @param {Object} customHeaders - Custom headers to merge
     * @param {boolean} includeAuth - Include authorization token
     * @param {boolean} includeCsrf - Include CSRF token
     * @returns {Object}
     */
    function buildHeaders(customHeaders = {}, includeAuth = true, includeCsrf = true) {
        const headers = { ...config.defaultHeaders, ...customHeaders };

        // Add CSRF token for non-GET requests
        if (includeCsrf) {
            const csrfToken = getCsrfToken();
            if (csrfToken) {
                headers['RequestVerificationToken'] = csrfToken;
            }
        }

        // Note: Authorization header (JWT) is automatically added by cookie
        // The server-side ApiClient reads it from the AuthToken cookie

        return headers;
    }

    /**
     * Handle API response
     * @param {Response} response - Fetch API response
     * @returns {Promise<any>}
     */
    async function handleResponse(response) {
        const contentType = response.headers.get('content-type');
        const isJson = contentType && contentType.includes('application/json');

        let data = null;
        if (isJson) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            // Handle different error status codes
            const error = new Error(data.message || data.title || 'An error occurred');
            error.status = response.status;
            error.statusText = response.statusText;
            error.data = data;

            // Handle specific status codes
            switch (response.status) {
                case 401:
                    // Unauthorized - redirect to login
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    break;
                case 403:
                    // Forbidden - show error
                    if (window.DocsAndPlannings && window.DocsAndPlannings.FlashMessage) {
                        window.DocsAndPlannings.FlashMessage.show('You do not have permission to perform this action.', 'error');
                    }
                    break;
                case 404:
                    error.message = 'Resource not found';
                    break;
                case 500:
                    error.message = 'Internal server error';
                    break;
            }

            throw error;
        }

        return data;
    }

    /**
     * Handle API error
     * @param {Error} error - Error object
     */
    function handleError(error) {
        console.error('API Error:', error);

        if (window.DocsAndPlannings && window.DocsAndPlannings.FlashMessage) {
            const message = error.message || 'An unexpected error occurred';
            window.DocsAndPlannings.FlashMessage.show(message, 'error');
        }

        throw error;
    }

    // ========================================
    // Public API Methods
    // ========================================

    /**
     * Make a GET request
     * @param {string} path - API endpoint path
     * @param {Object} options - Request options
     * @returns {Promise<any>}
     */
    async function get(path, options = {}) {
        const { headers = {}, timeout = config.defaultTimeout } = options;

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(buildUrl(path), {
                method: 'GET',
                headers: buildHeaders(headers, true, false), // No CSRF for GET
                signal: controller.signal,
                credentials: 'include' // Include cookies
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            return handleError(error);
        }
    }

    /**
     * Make a POST request
     * @param {string} path - API endpoint path
     * @param {any} data - Request body data
     * @param {Object} options - Request options
     * @returns {Promise<any>}
     */
    async function post(path, data, options = {}) {
        const { headers = {}, timeout = config.defaultTimeout } = options;

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(buildUrl(path), {
                method: 'POST',
                headers: buildHeaders(headers, true, true),
                body: JSON.stringify(data),
                signal: controller.signal,
                credentials: 'include'
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            return handleError(error);
        }
    }

    /**
     * Make a PUT request
     * @param {string} path - API endpoint path
     * @param {any} data - Request body data
     * @param {Object} options - Request options
     * @returns {Promise<any>}
     */
    async function put(path, data, options = {}) {
        const { headers = {}, timeout = config.defaultTimeout } = options;

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(buildUrl(path), {
                method: 'PUT',
                headers: buildHeaders(headers, true, true),
                body: JSON.stringify(data),
                signal: controller.signal,
                credentials: 'include'
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            return handleError(error);
        }
    }

    /**
     * Make a DELETE request
     * @param {string} path - API endpoint path
     * @param {Object} options - Request options
     * @returns {Promise<any>}
     */
    async function del(path, options = {}) {
        const { headers = {}, timeout = config.defaultTimeout } = options;

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(buildUrl(path), {
                method: 'DELETE',
                headers: buildHeaders(headers, true, true),
                signal: controller.signal,
                credentials: 'include'
            });

            clearTimeout(timeoutId);
            return await handleResponse(response);
        } catch (error) {
            return handleError(error);
        }
    }

    /**
     * Upload files via multipart/form-data
     * @param {string} path - API endpoint path
     * @param {FormData} formData - FormData object with files
     * @param {Object} options - Request options
     * @returns {Promise<any>}
     */
    async function upload(path, formData, options = {}) {
        const { headers = {}, timeout = config.defaultTimeout, onProgress = null } = options;

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            // Don't set Content-Type for FormData - browser will set it with boundary
            const uploadHeaders = buildHeaders(headers, true, true);
            delete uploadHeaders['Content-Type'];

            const xhr = new XMLHttpRequest();

            return new Promise((resolve, reject) => {
                xhr.upload.addEventListener('progress', (e) => {
                    if (e.lengthComputable && onProgress) {
                        const percentComplete = (e.loaded / e.total) * 100;
                        onProgress(percentComplete);
                    }
                });

                xhr.addEventListener('load', () => {
                    clearTimeout(timeoutId);
                    if (xhr.status >= 200 && xhr.status < 300) {
                        try {
                            const data = JSON.parse(xhr.responseText);
                            resolve(data);
                        } catch {
                            resolve(xhr.responseText);
                        }
                    } else {
                        reject(new Error(`Upload failed with status ${xhr.status}`));
                    }
                });

                xhr.addEventListener('error', () => {
                    clearTimeout(timeoutId);
                    reject(new Error('Upload failed'));
                });

                xhr.addEventListener('abort', () => {
                    clearTimeout(timeoutId);
                    reject(new Error('Upload aborted'));
                });

                xhr.open('POST', buildUrl(path));

                // Set headers
                Object.keys(uploadHeaders).forEach(key => {
                    xhr.setRequestHeader(key, uploadHeaders[key]);
                });

                xhr.withCredentials = true;
                xhr.send(formData);
            });
        } catch (error) {
            return handleError(error);
        }
    }

    /**
     * Set base URL for API requests
     * @param {string} url - Base URL
     */
    function setBaseUrl(url) {
        config.baseUrl = url;
    }

    /**
     * Set default timeout for requests
     * @param {number} timeout - Timeout in milliseconds
     */
    function setTimeout(timeout) {
        config.defaultTimeout = timeout;
    }

    /**
     * Set default headers
     * @param {Object} headers - Headers object
     */
    function setDefaultHeaders(headers) {
        config.defaultHeaders = { ...config.defaultHeaders, ...headers };
    }

    // ========================================
    // Public API
    // ========================================
    return {
        get: get,
        post: post,
        put: put,
        delete: del,
        upload: upload,
        setBaseUrl: setBaseUrl,
        setTimeout: setTimeout,
        setDefaultHeaders: setDefaultHeaders,
        config: config
    };
})();

// Make available globally
window.ApiClient = ApiClient;
