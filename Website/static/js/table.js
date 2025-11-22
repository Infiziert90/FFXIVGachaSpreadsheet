export function makeSortableTable(element, items, columns) {
    element.innerHTML = '';
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');

    const sort = {
        field: null,
        direction: null
    };

    const createTableBody = () => {
        let sortedItems = [...items];
        if (sort.field) {
            if (sort.direction === 'asc') {
                sortedItems = sortedItems.sort((a, b) => {
                    if (a[sort.field] == b[sort.field]) {
                        return 0;
                    } else if (a[sort.field] < b[sort.field]) {
                        return -1;
                    } else {
                        return 1;
                    }
                });
            } else {
                sortedItems = sortedItems.sort((a, b) => {
                    if (a[sort.field] == b[sort.field]) {
                        return 0;
                    } else if (a[sort.field] > b[sort.field]) {
                        return -1;
                    } else {
                        return 1;
                    }
                });
            }
        }

        const fragment = new DocumentFragment();
        for (const row of sortedItems) {
            const tr = document.createElement('tr');
            for (const column of columns) {
                const td = document.createElement('td');
                if (column.templateRenderer) {
                    td.innerHTML = column.templateRenderer(row);
                } else if (column.valueRenderer) {
                    td.innerText = column.valueRenderer(row);
                } else {
                    td.innerText = row[column.field];
                }

                if (column.classExtension) {
                    for(const extension of column.classExtension) {
                        td.classList.add(extension)
                    }
                }

                tr.appendChild(td);
            }
            fragment.appendChild(tr);
        }

        tbody.innerHTML = '';
        tbody.appendChild(fragment);
    };

    const changeSort = (column, direction) => {
        if (!column.field) {
            return;
        }

        if (direction) {
            sort.direction = direction;
        } else {
            if (sort.field === column.field) {
                sort.direction = sort.direction === 'asc' ? 'desc' : 'asc';
            } else {
                sort.direction = 'asc';
            }
        }
        sort.field = column.field;

        // update classes on <th> nodes
        for (const th of thead.childNodes[0].childNodes) {
            th.classList.remove('sorted-asc', 'sorted-desc');
            if (th.dataset.field && th.dataset.field === sort.field) {
                th.classList.add(sort.direction === 'asc' ? 'sorted-asc' : 'sorted-desc');
            }
        }

        // re-create the table body
        createTableBody();
    };

    // create table header
    const headerRow = document.createElement('tr');
    for (const column of columns) {
        const th = document.createElement('th');
        th.innerText = column.header;
        if (column.field) {
            th.dataset.field = column.field;
            th.classList.add('sortable');
            th.addEventListener('click', () => changeSort(column));

            if (column.classExtension) {
                for(const extension of column.classExtension) {
                    th.classList.add(extension);
                }
            }
        }
        headerRow.appendChild(th);
    }

    thead.appendChild(headerRow);
    element.appendChild(thead);

    // set default sort if we find it
    for (const column of columns) {
        if (column.defaultSort) {
            changeSort(column, column.defaultSort);
        }
    }

    createTableBody();
    element.appendChild(tbody);
}