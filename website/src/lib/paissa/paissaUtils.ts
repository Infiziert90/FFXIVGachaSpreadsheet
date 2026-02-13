import {PurchaseSystem} from "$lib/paissa/paissaStruct";

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