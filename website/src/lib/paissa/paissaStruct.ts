export interface WorldDetail {
    id: number;
    name: string;
    districts: DistrictDetail[];
    num_open_plots: number;
    oldest_plot_time: number;
}

export interface DistrictDetail {
    id: number;
    name: string;
    num_open_plots: number;
    oldest_plot_time: number;
    open_plots: OpenPlotDetail[];
}

export interface OpenPlotDetail {
    world_id: number;
    district_id: number;
    ward_number: number;
    plot_number: number;
    size: number;
    price: number;
    last_updated_time: number;
    first_seen_time: number;
    est_time_open_min: number;
    est_time_open_max: number;
    purchase_system: PurchaseSystem;
    lotto_entries: number | null;
    lotto_phase: number | null;
    lotto_phase_until: number | null;
}

export enum PurchaseSystem {
    LOTTERY = 1,
    FREE_COMPANY = 2,
    INDIVIDUAL = 4,
}