import type {Reward} from "$lib/interfaces";
import {error} from "@sveltejs/kit";
import type {HTMLThAttributes} from "svelte/elements";

export interface ColumnTemplate {
    header: string;

    sortable?: boolean;
    defaultSort?: string;

    templateRenderer?: Function;
    valueRenderer?: Function;
    field?: string;

    classExtension?: string[];
}

export function makeSortableTable(element: HTMLTableElement, items: Reward[], columns: ColumnTemplate[]) {
    element.innerHTML = '';
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');

    const sort = {
        field: '',
        direction: ''
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
                } else if (column.field) {
                    td.innerText = row[column.field];
                } else {
                    error(500, {message: 'No field or templateRenderer specified for column'});
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

    const changeSort = (column: ColumnTemplate, direction?: string) => {
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
        for (const th of Object.values(thead.childNodes[0].childNodes) as HTMLElement[]) {
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