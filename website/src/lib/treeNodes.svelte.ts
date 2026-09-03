export type TreeLevel = 'category' | 'expansion' | 'header' | 'territory' | 'fateType';

/**
 * Builds a unique path identifier for a tree node (e.g. "category-1/expansion-2").
 * A node is "top-level" (a root branch) exactly when its path has no parent segment.
 */
export function getPath(parentPath: string, id: number, level: TreeLevel): string {
    return parentPath ? `${parentPath}/${level}-${id}` : `${level}-${id}`;
}

/**
 * Filters a leaf list down to items whose name matches the query.
 */
export function filterLeaves<Item>(items: Item[], query: string, getName: (item: Item) => string): Item[] {
    return items.filter(item => getName(item).toLowerCase().includes(query));
}

/**
 * Filters a list of tree nodes down to those whose own name matches the query
 * (kept with children as-is), or that have at least one matching descendant
 * (kept with children replaced by the filtered subset).
 */
export function filterLevel<Item, Child>(
    items: Item[],
    query: string,
    getName: (item: Item) => string,
    getChildren: (item: Item) => Child[],
    withChildren: (item: Item, children: Child[]) => Item,
    filterChildren: (children: Child[], query: string) => Child[],
): Item[] {
    return items.reduce<Item[]>((acc, item) => {
        if (getName(item).toLowerCase().includes(query)) {
            acc.push(item);
            return acc;
        }
        const children = filterChildren(getChildren(item), query);
        if (children.length > 0) {
            acc.push(withChildren(item, children));
        }
        return acc;
    }, []);
}

const isTopLevel = (path: string): boolean => !path.includes('/');

/**
 * Tracks which tree node paths are open. Opening a top-level node closes any
 * other open top-level node first, so only one root branch stays expanded at a time.
 */
export function createTreeState() {
    let openNodes = $state<Set<string>>(new Set());

    function withOpen(nodes: Set<string>, path: string): Set<string> {
        if (isTopLevel(path)) {
            nodes = new Set(Array.from(nodes).filter(p => !isTopLevel(p)));
        }
        nodes.add(path);
        return nodes;
    }

    return {
        isOpen: (path: string): boolean => openNodes.has(path),
        toggle(path: string) {
            const next = new Set(openNodes);
            openNodes = next.delete(path) ? next : withOpen(next, path);
        },
        openExclusive(path: string) {
            openNodes = withOpen(new Set(openNodes), path);
        },
    };
}
