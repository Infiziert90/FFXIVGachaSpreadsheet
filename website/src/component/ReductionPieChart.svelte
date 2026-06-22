<script lang="ts">
    import type {ReductionSource} from "$lib/structs/reduction";
    import {Mappings} from "$lib/mappings";
    import {getIconPath} from "$lib/utils";
    import {SimpleReductionReward} from "$lib/sheets/simplifiedSheets";
    import * as echarts from "echarts";
    import {onMount} from "svelte";

    interface Props {
        source: ReductionSource;
        mainTier: number;
        onTierClick: (subTier: number) => void;
    }

    let { source, mainTier, onTierClick }: Props = $props();

    let el: HTMLDivElement;
    let chart: echarts.ECharts;

    function collectability(subTier: number): number {
        return SimpleReductionReward[mainTier]?.[subTier]?.MinimumCollectability ?? subTier;
    }

    function tierLabel(i: number): string {
        const min = collectability(source.Tiers[i].SubTier);
        const next = source.Tiers[i + 1]?.SubTier;
        return next !== undefined ? `${min}–${collectability(next) - 1}` : `${min}+`;
    }

    function tierItems(tier: ReductionSource['Tiers'][0]) {
        const map = new Map<number, { pct: number; count: number }>();
        for (const patch of Object.values(tier.Patches))
            for (const item of patch.Normal) {
                const ex = map.get(item.Id);
                if (ex) { ex.pct += item.Pct; ex.count++; }
                else map.set(item.Id, { pct: item.Pct, count: 1 });
            }
        return Array.from(map.entries()).map(([id, v]) => ({
            name: Mappings[id]?.Name ?? String(id),
            icon: getIconPath(Mappings[id]?.Icon ?? '', true),
            pct: Math.round((v.pct / v.count) * 100)
        }));
    }

    function siteFont(): string {
        return getComputedStyle(document.documentElement).getPropertyValue('--bs-body-font-family').trim();
    }

    function buildOption(): echarts.EChartsOption {
        const fontFamily = siteFont();
        const data = source.Tiers.map((tier, i) => ({
            value: tier.Records,
            name: tierLabel(i),
            minimum: tier.SubTier,
            items: tierItems(tier),
            pct: (tier.Records / source.Records * 100).toFixed(1)
        }));

        return {
            tooltip: {
                trigger: 'item',
                backgroundColor: 'var(--bs-body-bg)',
                borderColor: 'var(--bs-border-color)',
                borderWidth: 1,
                textStyle: { color: 'var(--bs-body-color)', fontSize: 13 },
                extraCssText: 'box-shadow: 0 4px 16px rgba(0,0,0,0.35); border-radius: 6px; font-family: inherit;',
                formatter(params: any) {
                    const d = params.data;
                    const rows = d.items.map((item: any) =>
                        `<div style="display:flex;align-items:center;gap:6px;margin-top:3px;">
                            <img src="${item.icon}" width="20" height="20" style="border-radius:2px;flex-shrink:0;">
                            <span style="flex:1;">${item.name}</span>
                            <span style="color:var(--bs-secondary-color);margin-left:12px;">${item.pct}%</span>
                        </div>`
                    ).join('');
                    return `<div style="min-width:160px;">
                        <div style="font-weight:700;font-size:0.9rem;">${d.name}</div>
                        <div style="color:var(--bs-secondary-color);margin-bottom:4px;">${d.value} records · ${d.pct}%</div>
                        <div style="border-top:1px solid var(--bs-border-color);padding-top:4px;">${rows}</div>
                    </div>`;
                }
            },
            legend: {
                orient: 'horizontal',
                bottom: 0,
                textStyle: { color: 'var(--bs-body-color)', fontSize: 12, fontFamily },
                formatter: (name: string) => `${name}  ${data.find(d => d.name === name)?.value ?? ''}`
            },
            series: [{
                type: 'pie',
                radius: ['0%', '68%'],
                center: ['50%', '46%'],
                data,
                label: { color: 'var(--bs-body-color)', formatter: '{b}\n{d}%', fontSize: 11, fontFamily },
                labelLine: { length: 10, length2: 8 },
                emphasis: { scale: true, scaleSize: 6, label: { fontSize: 13, fontWeight: 'bold' } }
            }]
        };
    }

    onMount(() => {
        chart = echarts.init(el, null, { renderer: 'svg' });
        chart.setOption(buildOption());
        chart.on('click', (params: any) => {
            if (params.data?.minimum !== undefined) onTierClick(params.data.minimum);
        });
        const ro = new ResizeObserver(() => chart.resize());
        ro.observe(el);
        return () => { ro.disconnect(); chart.dispose(); };
    });

    $effect(() => {
        source;
        if (chart) chart.setOption(buildOption(), { notMerge: true });
    });
</script>

<div bind:this={el} class="pie-container"></div>

<style>
    .pie-container { width: 100%; height: 480px; }
</style>
