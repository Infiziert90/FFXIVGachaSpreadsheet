import {type OpenPlotDetail, PurchaseSystem} from "$lib/paissa/paissaStruct";

export interface OpenPlot {
    Plot: number;
    Ward: number;
    Type: number;
    Tenant: PurchaseSystem;
    Bids: number | null;
    Phase: number | null;
    PhaseUntil: number | null;
}

export function createOpenPlot(plot: OpenPlotDetail): OpenPlot {
    return {
        Plot: plot.plot_number,
        Ward: plot.ward_number,
        Type: plot.size,
        Tenant: plot.purchase_system,
        Bids: plot.lotto_entries,
        Phase: plot.lotto_phase,
        PhaseUntil: plot.lotto_phase_until
    }
}

export function getPurchaseType(purchaseSystem: PurchaseSystem): string {
    switch (purchaseSystem) {
        case PurchaseSystem.LOTTERY + PurchaseSystem.INDIVIDUAL:
            return "Personal";
        case PurchaseSystem.LOTTERY + PurchaseSystem.FREE_COMPANY:
            return 'FC';
        case PurchaseSystem.FREE_COMPANY + PurchaseSystem.INDIVIDUAL:
        case PurchaseSystem.LOTTERY + PurchaseSystem.FREE_COMPANY + PurchaseSystem.INDIVIDUAL:
            return 'All';
        default:
            return 'Unknown';
    }
}

export function getPhaseOrBids(plot: OpenPlot): string {
    console.log(plot);

    if (plot.Phase === null || plot.PhaseUntil === null)
        return 'Missing Data';

    let now = Math.round(new Date().getTime() / 1000);
    if (now > plot.PhaseUntil)
        return 'Missing Data';

    if (plot.Phase === 2)
        return `Results`;

    if (plot.Phase === 3)
        return `Unavailable`;

    return plot.Bids?.toString() ?? 'Missing Data';
}