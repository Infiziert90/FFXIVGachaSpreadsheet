import moment from 'moment';

moment.updateLocale('en', {
    relativeTime : {
        future: "in %s",
        past:   "%s ago",
        s  : '%d seconds',
        ss : '%d seconds',
        m:  "%d minute",
        mm: "%d minutes",
        h:  "%d hour",
        hh: "%d hours",
        d:  "%d day",
        dd: "%d days"
    }
});

const StartBidDate = Date.UTC(2026, 2, 21, 15);

const BidLength = 5 * 24 * 60 * 60 * 1000;    // 5 Days
const ResultLength = 4 * 24 * 60 * 60 * 1000; // 4 Days

export enum Phase {
    Bid = 0,
    Result = 1,
}

export interface CurrentLotteryPhase {
    phase: Phase;
    currentEnd: number;
    nextStart: number;
}

export function getLotteryPhase(): CurrentLotteryPhase {
    let phase = Phase.Bid;

    let currentPhase = StartBidDate;
    let now = new Date().getTime();
    while (now > currentPhase + getPhaseLength(phase)) {
        switch (phase) {
            case Phase.Bid:
                phase = Phase.Result;
                currentPhase += BidLength;
                break;
            case Phase.Result:
                phase = Phase.Bid;
                currentPhase += ResultLength;
                break;
        }
    }

    return {phase: phase, currentEnd: currentPhase, nextStart: currentPhase + getPhaseLength(phase)};
}

function getPhaseLength(phase: Phase) {
    return phase === Phase.Bid ? BidLength : ResultLength;
}

export function getPhaseName(phase: Phase): string {
    return phase === Phase.Bid ? "Bid" : "Result";
}

export function getNextPhaseStart(next: number): string {
    return moment(next).format('LLLL');
}

export function getNextPhaseLeftover(next: number): string {
    return moment(next).fromNow();
}