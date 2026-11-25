export function init(element, dotNetRef) {
    if (!element || element._sortable) {
        return;
    }

    element._sortable = Sortable.create(element, {
        handle: '.drag-handle',
        animation: 150,
        forceFallback: true,
        fallbackOnBody: true,
        fallbackTolerance: 3,
        swapThreshold: 0.65,
        scroll: true,
        scrollSensitivity: 60,
        scrollSpeed: 12,
        ghostClass: 'todo-sortable-ghost',
        chosenClass: 'todo-sortable-chosen',
        dragClass: 'todo-sortable-drag',
        onEnd: async (evt) => {
            if (evt.oldIndex == null || evt.newIndex == null || evt.oldIndex === evt.newIndex) {
                return;
            }

            // Revert DOM move so Blazor's renderer stays in sync before StateHasChanged
            const parent = evt.from;
            const item = evt.item;
            parent.removeChild(item);
            const refNode = parent.children[evt.oldIndex] || null;
            parent.insertBefore(item, refNode);

            await dotNetRef.invokeMethodAsync('OnItemReordered', evt.oldIndex, evt.newIndex);
        }
    });
}

export function dispose(element) {
    if (element && element._sortable) {
        element._sortable.destroy();
        delete element._sortable;
    }
}

