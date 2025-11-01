/**
 * Kanban Board JavaScript Module
 * Handles drag-and-drop, quick edit, column settings, and board interactions
 */

const KanbanBoard = (function () {
    'use strict';

    let config = {
        projectId: null,
        boardId: null
    };

    let draggedCard = null;
    let draggedWorkItemId = null;
    let draggedFromStatusId = null;

    /**
     * Initialize the kanban board
     */
    function init(options) {
        config = { ...config, ...options };

        initializeDragAndDrop();
        initializeQuickEdit();
        initializeColumnSettings();
        initializeColumnCollapse();
    }

    /**
     * Initialize drag and drop functionality for work item cards
     */
    function initializeDragAndDrop() {
        const cards = document.querySelectorAll('.kanban-card');
        const columns = document.querySelectorAll('.kanban-column-body');

        // Make cards draggable
        cards.forEach(card => {
            card.draggable = true;

            card.addEventListener('dragstart', function (e) {
                draggedCard = this;
                draggedWorkItemId = parseInt(this.getAttribute('data-work-item-id'));
                draggedFromStatusId = parseInt(this.getAttribute('data-status-id'));

                this.classList.add('dragging');
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/html', this.innerHTML);
            });

            card.addEventListener('dragend', function (e) {
                this.classList.remove('dragging');
                draggedCard = null;
                draggedWorkItemId = null;
                draggedFromStatusId = null;
            });
        });

        // Make columns drop targets
        columns.forEach(column => {
            column.addEventListener('dragover', function (e) {
                e.preventDefault();
                e.dataTransfer.dropType = 'move';
                this.classList.add('drag-over');
            });

            column.addEventListener('dragleave', function (e) {
                this.classList.remove('drag-over');
            });

            column.addEventListener('drop', function (e) {
                e.preventDefault();
                this.classList.remove('drag-over');

                if (draggedCard) {
                    const newStatusId = parseInt(this.getAttribute('data-status-id'));

                    // Only move if status changed
                    if (newStatusId !== draggedFromStatusId) {
                        // Add card to new column visually (optimistic UI update)
                        this.insertBefore(draggedCard, this.firstChild);

                        // Update card's status ID
                        draggedCard.setAttribute('data-status-id', newStatusId);

                        // Send API request to move work item
                        moveWorkItem(draggedWorkItemId, newStatusId, draggedFromStatusId);
                    }
                }
            });
        });
    }

    /**
     * Move a work item to a new status
     */
    function moveWorkItem(workItemId, newStatusId, oldStatusId) {
        const data = {
            newStatusId: newStatusId
        };

        ApiClient.put(`/api/projects/${config.projectId}/board/workitems/${workItemId}/move`, data)
            .then(response => {
                if (response && response.success) {
                    FlashMessage.success('Work item moved successfully');
                    updateColumnCounts();
                } else {
                    FlashMessage.error(response.message || 'Failed to move work item');
                    // Reload page to restore correct state
                    window.location.reload();
                }
            })
            .catch(error => {
                console.error('Error moving work item:', error);
                FlashMessage.error('Failed to move work item');
                // Reload page to restore correct state
                window.location.reload();
            });
    }

    /**
     * Update column item counts after drag and drop
     */
    function updateColumnCounts() {
        const columns = document.querySelectorAll('.kanban-column');

        columns.forEach(column => {
            const body = column.querySelector('.kanban-column-body');
            const cards = body ? body.querySelectorAll('.kanban-card') : [];
            const itemCount = cards.length;

            const countElement = column.querySelector('.kanban-column-header small');
            if (countElement) {
                const wipLimitMatch = countElement.textContent.match(/\/ (\d+)/);
                if (wipLimitMatch) {
                    countElement.textContent = `${itemCount} / ${wipLimitMatch[1]} items`;
                } else {
                    countElement.textContent = `${itemCount} items`;
                }
            }
        });
    }

    /**
     * Initialize quick edit functionality
     */
    function initializeQuickEdit() {
        const quickEditModal = document.getElementById('quickEditModal');
        if (!quickEditModal) return;

        const modal = new bootstrap.Modal(quickEditModal);
        const saveBtn = document.getElementById('saveQuickEditBtn');

        // Quick edit trigger buttons
        document.querySelectorAll('.quick-edit-trigger').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const workItemId = parseInt(this.getAttribute('data-work-item-id'));
                loadWorkItemForEdit(workItemId, modal);
            });
        });

        // Save button
        if (saveBtn) {
            saveBtn.addEventListener('click', function () {
                saveQuickEdit(modal);
            });
        }
    }

    /**
     * Load work item data for quick edit
     */
    function loadWorkItemForEdit(workItemId, modal) {
        LoadingSpinner.show();

        ApiClient.get(`/Kanban/GetWorkItemForEdit/${workItemId}`)
            .then(data => {
                if (data) {
                    populateQuickEditForm(data);
                    modal.show();
                } else {
                    FlashMessage.error('Failed to load work item');
                }
            })
            .catch(error => {
                console.error('Error loading work item:', error);
                FlashMessage.error('Failed to load work item');
            })
            .finally(() => {
                LoadingSpinner.hide();
            });
    }

    /**
     * Populate quick edit form with work item data
     */
    function populateQuickEditForm(data) {
        document.getElementById('editWorkItemId').value = data.id;
        document.getElementById('editWorkItemKey').value = data.key;
        document.getElementById('editSummary').value = data.summary;
        document.getElementById('editPriority').value = data.priority;

        // Populate status dropdown
        const statusSelect = document.getElementById('editStatusId');
        statusSelect.innerHTML = '';
        data.availableStatuses.forEach(status => {
            const option = document.createElement('option');
            option.value = status.id;
            option.textContent = status.name;
            option.selected = status.id === data.statusId;
            statusSelect.appendChild(option);
        });

        // Populate assignee dropdown
        const assigneeSelect = document.getElementById('editAssigneeId');
        assigneeSelect.innerHTML = '<option value="">Unassigned</option>';
        data.availableUsers.forEach(user => {
            const option = document.createElement('option');
            option.value = user.id;
            option.textContent = `${user.firstName} ${user.lastName}`;
            option.selected = user.id === data.assigneeId;
            assigneeSelect.appendChild(option);
        });
    }

    /**
     * Save quick edit changes
     */
    function saveQuickEdit(modal) {
        const workItemId = parseInt(document.getElementById('editWorkItemId').value);
        const summary = document.getElementById('editSummary').value.trim();
        const statusId = parseInt(document.getElementById('editStatusId').value);
        const assigneeIdValue = document.getElementById('editAssigneeId').value;
        const assigneeId = assigneeIdValue ? parseInt(assigneeIdValue) : null;
        const priority = parseInt(document.getElementById('editPriority').value);

        if (!summary) {
            FlashMessage.error('Summary is required');
            return;
        }

        const data = {
            id: workItemId,
            summary: summary,
            statusId: statusId,
            assigneeId: assigneeId,
            priority: priority
        };

        LoadingSpinner.show();

        ApiClient.post('/Kanban/QuickUpdateWorkItem', data)
            .then(response => {
                if (response && response.success) {
                    FlashMessage.success('Work item updated successfully');
                    modal.hide();
                    // Reload page to show updated data
                    window.location.reload();
                } else {
                    FlashMessage.error(response.message || 'Failed to update work item');
                }
            })
            .catch(error => {
                console.error('Error updating work item:', error);
                FlashMessage.error('Failed to update work item');
            })
            .finally(() => {
                LoadingSpinner.hide();
            });
    }

    /**
     * Initialize column settings functionality
     */
    function initializeColumnSettings() {
        const settingsModal = document.getElementById('columnSettingsModal');
        if (!settingsModal) return;

        const modal = new bootstrap.Modal(settingsModal);
        const saveBtn = document.getElementById('saveColumnSettingsBtn');

        // Column settings trigger buttons
        document.querySelectorAll('.column-settings-trigger').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const columnId = parseInt(this.getAttribute('data-column-id'));
                const columnName = this.getAttribute('data-column-name');
                const wipLimit = this.getAttribute('data-wip-limit');
                const isCollapsed = this.getAttribute('data-is-collapsed') === 'true';

                populateColumnSettingsForm(columnId, columnName, wipLimit, isCollapsed);
                modal.show();
            });
        });

        // Save button
        if (saveBtn) {
            saveBtn.addEventListener('click', function () {
                saveColumnSettings(modal);
            });
        }
    }

    /**
     * Populate column settings form
     */
    function populateColumnSettingsForm(columnId, columnName, wipLimit, isCollapsed) {
        document.getElementById('settingsColumnId').value = columnId;
        document.getElementById('settingsColumnName').value = columnName;
        document.getElementById('settingsWIPLimit').value = wipLimit && wipLimit !== 'null' ? wipLimit : '';
        document.getElementById('settingsIsCollapsed').checked = isCollapsed;
    }

    /**
     * Save column settings
     */
    function saveColumnSettings(modal) {
        const columnId = parseInt(document.getElementById('settingsColumnId').value);
        const columnName = document.getElementById('settingsColumnName').value;
        const wipLimitValue = document.getElementById('settingsWIPLimit').value.trim();
        const wipLimit = wipLimitValue ? parseInt(wipLimitValue) : null;
        const isCollapsed = document.getElementById('settingsIsCollapsed').checked;

        const data = {
            id: columnId,
            name: columnName,
            wipLimit: wipLimit,
            isCollapsed: isCollapsed
        };

        LoadingSpinner.show();

        ApiClient.post(`/Kanban/UpdateColumn/${config.projectId}/${columnId}`, data)
            .then(response => {
                if (response && response.success) {
                    FlashMessage.success('Column settings updated successfully');
                    modal.hide();
                    // Reload page to show updated settings
                    window.location.reload();
                } else {
                    FlashMessage.error(response.message || 'Failed to update column settings');
                }
            })
            .catch(error => {
                console.error('Error updating column settings:', error);
                FlashMessage.error('Failed to update column settings');
            })
            .finally(() => {
                LoadingSpinner.hide();
            });
    }

    /**
     * Initialize column collapse/expand functionality
     */
    function initializeColumnCollapse() {
        document.querySelectorAll('.toggle-collapse-trigger').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const columnId = parseInt(this.getAttribute('data-column-id'));
                const column = document.querySelector(`.kanban-column[data-column-id="${columnId}"]`);

                if (column) {
                    const isCollapsed = column.classList.contains('kanban-column-collapsed');
                    toggleColumnCollapse(columnId, !isCollapsed);
                }
            });
        });
    }

    /**
     * Toggle column collapse state
     */
    function toggleColumnCollapse(columnId, isCollapsed) {
        const column = document.querySelector(`.kanban-column[data-column-id="${columnId}"]`);
        if (!column) return;

        const columnName = column.querySelector('.kanban-column-header h5').textContent.trim();
        const currentWIPLimit = column.querySelector('[data-wip-limit]')?.getAttribute('data-wip-limit');

        const data = {
            id: columnId,
            name: columnName,
            wipLimit: currentWIPLimit && currentWIPLimit !== 'null' ? parseInt(currentWIPLimit) : null,
            isCollapsed: isCollapsed
        };

        LoadingSpinner.show();

        ApiClient.post(`/Kanban/UpdateColumn/${config.projectId}/${columnId}`, data)
            .then(response => {
                if (response && response.success) {
                    // Toggle UI state
                    if (isCollapsed) {
                        column.classList.add('kanban-column-collapsed');
                    } else {
                        column.classList.remove('kanban-column-collapsed');
                    }
                    FlashMessage.success(`Column ${isCollapsed ? 'collapsed' : 'expanded'} successfully`);
                } else {
                    FlashMessage.error(response.message || 'Failed to toggle column collapse');
                }
            })
            .catch(error => {
                console.error('Error toggling column collapse:', error);
                FlashMessage.error('Failed to toggle column collapse');
            })
            .finally(() => {
                LoadingSpinner.hide();
            });
    }

    // Public API
    return {
        init: init
    };
})();
