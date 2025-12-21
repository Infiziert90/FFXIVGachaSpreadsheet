export enum SectorType {
    UnknownUnlock = 9876,
    Begin = 9000,
    Map = 9999
}

export interface UnlockedFrom {
    Sector: number;
    Sub: boolean;
    Map: boolean;
    Main: boolean;
}

export function CreateUnlockedFrom(sector: number, main: boolean = false, map: boolean = false, sub: boolean = false): UnlockedFrom {
    return { Sector: sector, Sub: sub, Map: map, Main: main}
}

export const SectorToUnlock: Record<number, UnlockedFrom> =
{
    1: CreateUnlockedFrom(SectorType.Begin as number),                    // A    Default
    2: CreateUnlockedFrom(SectorType.Begin as number),                    // B    Default
    3: CreateUnlockedFrom(1),                             // C    Deep-sea Site 2		        <-      The Ivory Shoals
    4: CreateUnlockedFrom(2),                             // D    The Lightless Basin		    <-      Deep-sea Site 1
    5: CreateUnlockedFrom(2, true),                 // E    Deep-sea Site 3		        <-      Deep-sea Site 1
    6: CreateUnlockedFrom(3),                             // F    The Southern Rimilala Trench	<-      Deep-sea Site 2
    7: CreateUnlockedFrom(4),                             // G    The Umbrella Narrow		    <-      The Lightless Basin
    8: CreateUnlockedFrom(7),                             // H    Offender's Rot		        <-      The Umbrella Narrow
    9: CreateUnlockedFrom(5),                             // I    Neolith Island		        <-      Deep-sea Site 3
    10: CreateUnlockedFrom(5, true, false, true),     // J    Unidentified Derelict		    <-      Deep-sea Site 3
    11: CreateUnlockedFrom(9),                            // K    The Cobalt Shoals		        <-      Neolith Island
    12: CreateUnlockedFrom(8),                            // L    The Mystic Basin		        <-      Offender's Rot
    13: CreateUnlockedFrom(8),                            // M    Deep-sea Site 4		        <-      Offender's Rot
    14: CreateUnlockedFrom(10, true),               // N    The Central Rimilala Trench	<-      Unidentified Derelict
    15: CreateUnlockedFrom(14, true, false, true),    // O    The Wreckage Of Discovery I	<-      The Central Rimilala Trench
    16: CreateUnlockedFrom(11),                           // P    Komura		                <-      The Cobalt Shoals
    17: CreateUnlockedFrom(16),                           // Q    Kanayama		                <-      Komura
    18: CreateUnlockedFrom(12),                           // R    Concealed Bay		            <-      The Mystic Basin
    19: CreateUnlockedFrom(15, true),               // S    Deep-sea Site 5		        <-      The Wreckage Of Discovery I
    20: CreateUnlockedFrom(19, true, false, true),    // T    Purgatory		                <-      Deep-sea Site 5
    21: CreateUnlockedFrom(19),                           // U    Deep-sea Site 6		        <-      Deep-sea Site 5
    22: CreateUnlockedFrom(21),                           // V    The Rimilala Shelf		    <-      Deep-sea Site 6
    23: CreateUnlockedFrom(14),                           // W    Deep-sea Site 7		        <-      The Central Rimilala Trench
    24: CreateUnlockedFrom(23),                           // X    Glittersand Basin		        <-      Deep-sea Site 7
    25: CreateUnlockedFrom(20, true),               // Y    Flickering Dip		        <-      Purgatory
    26: CreateUnlockedFrom(25, true),               // Z    The Wreckage Of The Headway	<-      Flickering Dip
    27: CreateUnlockedFrom(26, true),               // AA   The Upwell		            <-      The Wreckage Of The Headway
    28: CreateUnlockedFrom(27, true),               // AB   The Rimilala Trench Bottom    <-      The Upwell
    29: CreateUnlockedFrom(27),                           // AC   Stone Temple		            <-      The Upwell
    30: CreateUnlockedFrom(28, true, true),    // AD   Sunken Vault		            <-      The Rimilala Trench Bottom

    32: CreateUnlockedFrom(30, true),               // A    South Isle Of Zozonan		    <- 		Sunken Vault
    33: CreateUnlockedFrom(32, true),               // B    Wreckage Of The Windwalker	<- 		South Isle Of Zozonan
    34: CreateUnlockedFrom(33, true),               // C    North Isle Of Zozonan		    <- 		Wreckage Of The Windwalker
    35: CreateUnlockedFrom(34),                           // D    Sea Of Ash 1		            <- 		North Isle Of Zozonan
    36: CreateUnlockedFrom(35),                           // E    The Southern Charnel Trench   <- 		Sea Of Ash 1
    37: CreateUnlockedFrom(34, true),               // F    Sea Of Ash 2		            <- 		North Isle Of Zozonan
    38: CreateUnlockedFrom(37, true),               // G    Sea Of Ash 3		            <- 		Sea Of Ash 2
    39: CreateUnlockedFrom(38, true),               // H    Ascetic's Demise		        <- 		Sea Of Ash 3
    40: CreateUnlockedFrom(38),                           // I    The Central Charnel Trench	<- 		Sea Of Ash 3
    41: CreateUnlockedFrom(40),                           // J    The Catacombs Of The Father	<- 		The Central Charnel Trench
    42: CreateUnlockedFrom(39, true),               // K    Sea Of Ash 4		            <- 		Ascetic's Demise
    43: CreateUnlockedFrom(42, true),               // L    The Midden Pit		        <- 		Sea Of Ash 4
    44: CreateUnlockedFrom(40),                           // M    The Lone Glove		        <- 		The Central Charnel Trench
    45: CreateUnlockedFrom(41),                           // N    Coldtoe Isle	               	<- 		The Catacombs Of The Father
    46: CreateUnlockedFrom(45),                           // O    Smuggler's Knot		        <- 		Coldtoe Isle
    47: CreateUnlockedFrom(43, true),               // P    The Open Robe	                <- 		The Midden Pit
    48: CreateUnlockedFrom(36),                           // Q    Nald'thal's Pipe	            <- 		The Southern Charnel Trench
    49: CreateUnlockedFrom(47, true, true),    // R    The Slipped Anchor	        <- 		The Open Robe
    50: CreateUnlockedFrom(45),                           // S    Glutton's Belly	            <- 		Coldtoe Isle
    51: CreateUnlockedFrom(42),                           // T    The Blue Hole		            <- 		Sea Of Ash 4

    53: CreateUnlockedFrom(49, true),               // A    The Isle Of Sacrament		    <- 		The Slipped Anchor
    54: CreateUnlockedFrom(53),                           // B    The Kraken's Tomb		        <- 		The Isle Of Sacrament
    55: CreateUnlockedFrom(53, true),               // C    Sea Of Jade 1		            <- 		The Isle Of Sacrament
    56: CreateUnlockedFrom(55),                           // D    Rogo-Tumu-Here's Haunt		<- 		Sea Of Jade 1
    57: CreateUnlockedFrom(55, true),               // E    The Stone Barbs		        <- 		Sea Of Jade 1
    58: CreateUnlockedFrom(56),                           // F    Rogo-Tumu-Here's Repose		<- 		Rogo-Tumu-Here's Haunt
    59: CreateUnlockedFrom(57, true),               // G    Tangaroa's Prow		        <- 		The Stone Barbs
    60: CreateUnlockedFrom(57),                           // H    Sea Of Jade 2		            <- 		The Stone Barbs
    61: CreateUnlockedFrom(59),                           // I    The Blind Sound		        <- 		Tangaroa's Prow
    62: CreateUnlockedFrom(59, true),               // J    Sea Of Jade 3		            <- 		Tangaroa's Prow
    63: CreateUnlockedFrom(61),                           // K    Moergynn's Forge		        <- 		The Blind Sound
    64: CreateUnlockedFrom(61),                           // L    Tangaroa's Beacon		        <- 		The Blind Sound
    65: CreateUnlockedFrom(62, true),               // M    Sea Of Jade 4		            <- 		Sea Of Jade 3
    66: CreateUnlockedFrom(65),                           // N    The Forest Of Kelp		    <- 		Sea Of Jade 4
    67: CreateUnlockedFrom(64),                           // O    Sea Of Jade 5		            <- 		Tangaroa's Beacon
    68: CreateUnlockedFrom(66),                           // P    Bladefall Chasm		        <- 		The Forest Of Kelp
    69: CreateUnlockedFrom(64),                           // Q    Stormport		                <- 		Tangaroa's Beacon
    70: CreateUnlockedFrom(65, true),               // R    Wyrm's Rest		            <- 		Sea Of Jade 4
    71: CreateUnlockedFrom(69),                           // S    Sea Of Jade 6		            <- 		Stormport
    72: CreateUnlockedFrom(70, true, true),    // T    The Devil's Crypt		        <- 		Wyrm's Rest

    74: CreateUnlockedFrom(72, true),               // A    Mastbound's Bounty		    <- 		The Devil's Crypt
    75: CreateUnlockedFrom(74, true),               // B    Sirensong Sea 1		        <- 		Mastbound's Bounty
    76: CreateUnlockedFrom(74),                           // C    Sirensong Sea 2		        <- 		Mastbound's Bounty
    77: CreateUnlockedFrom(76),                           // D    Anthemoessa		            <- 		Sirensong Sea 2
    78: CreateUnlockedFrom(75),                           // E    Magos Trench		            <- 		Sirensong Sea 1
    79: CreateUnlockedFrom(75, true),               // F    Thrall's Unrest		        <- 		Sirensong Sea 1
    80: CreateUnlockedFrom(76),                           // G    Crow's Drop		            <- 		Sirensong Sea 2
    81: CreateUnlockedFrom(77),                           // H    Sirensong Sea 3		        <- 		Anthemoessa
    82: CreateUnlockedFrom(81),                           // I    The Anthemoessa Undertow		<- 		Sirensong Sea 3
    83: CreateUnlockedFrom(79, true),               // J    Sirensong Sea 4		        <- 		Thrall's Unrest
    84: CreateUnlockedFrom(83),                           // K    Seafoam Tide		            <- 		Sirensong Sea 4
    85: CreateUnlockedFrom(83, true),               // L    The Beak		                <- 		Sirensong Sea 4
    86: CreateUnlockedFrom(81),                           // M    Seafarer's End		        <- 		Sirensong Sea 3
    87: CreateUnlockedFrom(82),                           // N    Drifter's Decay		        <- 		The Anthemoessa Undertow
    88: CreateUnlockedFrom(84),                           // O    Lugat's Landing		        <- 		Seafoam Tide
    89: CreateUnlockedFrom(85, true),               // P    The Frozen Spring		        <- 		The Beak
    90: CreateUnlockedFrom(87),                           // Q    Sirensong Sea 5		        <- 		Drifter's Decay
    91: CreateUnlockedFrom(88),                           // R    Tidewind Isle		            <- 		Lugat's Landing
    92: CreateUnlockedFrom(88),                           // S    Bloodbreak		            <- 		Lugat's Landing
    93: CreateUnlockedFrom(89, true, true),    // T    The Crystal Font		        <- 		The Frozen Spring

    95: CreateUnlockedFrom(93, true),               // A    Weeping Trellis               <-       The Crystal Font
    96: CreateUnlockedFrom(95, true),               // B    The Forsaken Isle             <-       Weeping Trellis
    97: CreateUnlockedFrom(95),                           // C    Fortune's Ford                <-       Weeping Trellis
    98: CreateUnlockedFrom(96),                           // D    The Lilac Sea 1               <-       The Forsaken Isle
    99: CreateUnlockedFrom(97),                           // E    Runner's Reach                <-       Fortune's Ford
    100: CreateUnlockedFrom(96, true),              // F    Bellflower Flood              <-       The Forsaken Isle
    101: CreateUnlockedFrom(97),                          // G    The Lilac Sea 2               <-       Fortune's Ford
    102: CreateUnlockedFrom(101),                         // H    Lilac Sea 3                   <-       Lilac Sea 2
    103: CreateUnlockedFrom(98),                          // I    Northwest Bellflower          <-       Lilac Sea 1
    104: CreateUnlockedFrom(100, true),             // J    Corolla Isle                  <-       Bellflower Flood
    105: CreateUnlockedFrom(101),                         // K    Southeast Bellflower          <-       Lilac Sea 2
    106: CreateUnlockedFrom(104, true),             // L    The Floral Reef               <-       Corolla Isle
    107: CreateUnlockedFrom(105),                         // M    Wingsreach                    <-       Southeast Bellflower
    108: CreateUnlockedFrom(106),                         // N    The Floating Standard         <-       The Floral Reef
    109: CreateUnlockedFrom(107),                         // O    The Fluttering Bay            <-       Wingsreach
    110: CreateUnlockedFrom(103),                         // P    Lilac Sea 4                   <-       Northwest Bellflower
    111: CreateUnlockedFrom(106, true),             // Q    Proudkeel                     <-       The Floral Reef
    112: CreateUnlockedFrom(109),                         // R    East Dodie's Abyss            <-       The Fluttering Bay
    113: CreateUnlockedFrom(108),                         // S    Lilac Sea 5                   <-       The Floating Standard
    114: CreateUnlockedFrom(111, true, true),  // T    West Dodie's Abyss            <-       Proudkeel

    116: CreateUnlockedFrom(114),                         // A    The Indigo Shallows           <-       West Dodie's Abyss
    117: CreateUnlockedFrom(116, true),             // B    Voyagers' Reprieve            <-       The Indigo Shallows
    118: CreateUnlockedFrom(116),                         // C    North Delphinium Seashelf     <-       The Indigo Shallows
    119: CreateUnlockedFrom(117),                         // D    Rainbringer Rift              <-       Voyagers' Reprieve
    120: CreateUnlockedFrom(118),                         // E    South Indigo Deep 1           <-       North Delphinium Seashelf
    121: CreateUnlockedFrom(117, true),             // F    The Central Blue              <-       Voyagers' Reprieve
    122: CreateUnlockedFrom(118),                         // G    South Indigo Deep 2           <-       North Delphinium Seashelf
    123: CreateUnlockedFrom(122),                         // H    The Talon                     <-       South Indigo Deep 2
    124: CreateUnlockedFrom(121, true),             // I    Southern Central Blue         <-       The Central Blue
    125: CreateUnlockedFrom(122),                         // J    South Indigo Deep 3           <-       South Indigo Deep 2
    126: CreateUnlockedFrom(123),                         // K    the Talonspoint Depths        <-       The Talon
    127: CreateUnlockedFrom(124),                         // L    Saltfarer's Eye               <-       Southern Central Blue
    128: CreateUnlockedFrom(124, true),             // M    Startail Shallows             <-       Southern Central Blue
    129: CreateUnlockedFrom(128, true),             // N    Moonshadow Isle               <-       Startail Shallows
    130: CreateUnlockedFrom(127),                         // O    Emerald Drop                  <-       Saltfarer's Eye
    131: CreateUnlockedFrom(129),                         // P    South Indigo Deep 4           <-       Moonshadow Isle
    132: CreateUnlockedFrom(127),                         // Q    South Delphinium Seashelf     <-       Saltfarer's Eye
    133: CreateUnlockedFrom(129, true),             // R    Startail Shelf                <-       Moonshadow Isle
    134: CreateUnlockedFrom(132),                         // S    Cradle of the Winds           <-       South Delphinium Seashelf
    135: CreateUnlockedFrom(133, true, true),  // T    Startail Trench               <-       Startail Shelf

    137: CreateUnlockedFrom(135),                         // A    Eastern Blackblood Wells        <-       Startail Trench
    138: CreateUnlockedFrom(137),                         // B    Sea Wolf Cove                   <-       Eastern Blackblood Wells
    139: CreateUnlockedFrom(137),                         // C    Southernmost Hanthbyrt          <-       Eastern Blackblood Wells
    140: CreateUnlockedFrom(139),                         // D    Oeyaseik                        <-       Southernmost Hanthbyrt
    141: CreateUnlockedFrom(138),                         // E    Northeast Hanthbyrt             <-      Sea Wolf Cove
    142: CreateUnlockedFrom(140),                         // F    Vyrstrant                       <-       Oeyaseik
    143: CreateUnlockedFrom(SectorType.UnknownUnlock as number),          // G    The Sunken Jawbone (G)          <-
}

export interface Breakpoint {
    T2: number;
    T3: number;
    Normal: number;
    Optimal: number;
    Favor: number;
}

function CreateBreakpoint(t2: number, t3: number, normal: number, optimal: number, favor: number): Breakpoint {
    return { T2: t2, T3: t3, Normal: normal, Optimal: optimal, Favor: favor}
}

export function EmptyBreakpoint(): Breakpoint {
    return { T2: 0, T3: 0, Normal: 0, Optimal: 0, Favor: 0 };
}

export const MapBreakpoints: Record<number, Breakpoint> =
{
    1: CreateBreakpoint(20, 80, 50, 80, 70),
    2: CreateBreakpoint(20, 80, 50, 80, 70),
    3: CreateBreakpoint(20, 85, 55, 85, 70),
    4: CreateBreakpoint(20, 85, 55, 85, 70),
    5: CreateBreakpoint(25, 90, 60, 90, 80),
    6: CreateBreakpoint(25, 90, 60, 90, 80),
    7: CreateBreakpoint(30, 95, 65, 95, 90),
    8: CreateBreakpoint(30, 100, 70, 100, 90),
    9: CreateBreakpoint(35, 110, 75, 105, 90),
    10: CreateBreakpoint(50, 115, 80, 110, 90),
    11: CreateBreakpoint(50, 90, 80, 110, 70),
    12: CreateBreakpoint(55, 95, 90, 120, 80),
    13: CreateBreakpoint(60, 100, 100, 130, 75),
    14: CreateBreakpoint(60, 100, 100, 130, 85),
    15: CreateBreakpoint(80, 115, 120, 160, 90),
    16: CreateBreakpoint(60, 100, 100, 130, 85),
    17: CreateBreakpoint(65, 105, 110, 140, 90),
    18: CreateBreakpoint(85, 120, 135, 175, 95),
    19: CreateBreakpoint(75, 110, 120, 155, 95),
    20: CreateBreakpoint(90, 125, 140, 180, 100),
    21: CreateBreakpoint(90, 120, 135, 175, 95),
    22: CreateBreakpoint(105, 130, 140, 180, 100),
    23: CreateBreakpoint(110, 140, 140, 180, 105),
    24: CreateBreakpoint(120, 130, 145, 190, 105),
    25: CreateBreakpoint(120, 135, 145, 190, 105),
    26: CreateBreakpoint(135, 140, 150, 195, 110),
    27: CreateBreakpoint(130, 145, 150, 195, 110),
    28: CreateBreakpoint(130, 150, 155, 200, 120),
    29: CreateBreakpoint(135, 150, 160, 200, 130),
    30: CreateBreakpoint(140, 155, 170, 215, 135),

    32: CreateBreakpoint(135, 150, 165, 205, 140),
    33: CreateBreakpoint(140, 155, 170, 205, 140),
    34: CreateBreakpoint(140, 160, 175, 210, 145),
    35: CreateBreakpoint(145, 165, 180, 220, 145),
    36: CreateBreakpoint(145, 160, 185, 220, 150),
    37: CreateBreakpoint(145, 165, 180, 220, 145),
    38: CreateBreakpoint(150, 170, 180, 220, 140),
    39: CreateBreakpoint(160, 175, 190, 225, 150),
    40: CreateBreakpoint(155, 170, 190, 220, 140),
    41: CreateBreakpoint(160, 175, 190, 225, 150),
    42: CreateBreakpoint(155, 170, 185, 230, 160),
    43: CreateBreakpoint(160, 175, 185, 235, 165),
    44: CreateBreakpoint(160, 170, 190, 240, 175),
    45: CreateBreakpoint(165, 190, 195, 245, 170),
    46: CreateBreakpoint(170, 185, 205, 250, 175),
    47: CreateBreakpoint(165, 180, 185, 235, 165),
    48: CreateBreakpoint(165, 180, 185, 235, 165),
    49: CreateBreakpoint(170, 185, 190, 240, 165),
    50: CreateBreakpoint(175, 190, 200, 250, 175),
    51: CreateBreakpoint(180, 190, 200, 250, 175),

    53: CreateBreakpoint(180, 190, 200, 250, 175),
    54: CreateBreakpoint(180, 190, 200, 250, 175),
    55: CreateBreakpoint(180, 190, 200, 250, 175),
    56: CreateBreakpoint(180, 195, 205, 260, 178),
    57: CreateBreakpoint(180, 195, 210, 260, 185),
    58: CreateBreakpoint(180, 195, 210, 265, 185),
    59: CreateBreakpoint(180, 195, 215, 270, 185),
    60: CreateBreakpoint(180, 195, 220, 270, 185),
    61: CreateBreakpoint(180, 195, 220, 270, 185),
    62: CreateBreakpoint(180, 195, 220, 270, 185),
    63: CreateBreakpoint(185, 200, 225, 275, 190),
    64: CreateBreakpoint(185, 200, 230, 280, 190),
    65: CreateBreakpoint(185, 200, 230, 280, 190),
    66: CreateBreakpoint(190, 205, 235, 285, 195),
    67: CreateBreakpoint(195, 210, 240, 290, 200),
    68: CreateBreakpoint(195, 210, 245, 295, 200),
    69: CreateBreakpoint(200, 215, 255, 300, 205),
    70: CreateBreakpoint(205, 220, 255, 300, 210),
    71: CreateBreakpoint(205, 220, 260, 305, 210),
    72: CreateBreakpoint(205, 220, 260, 305, 210),

    74: CreateBreakpoint(205, 220, 260, 305, 210),
    75: CreateBreakpoint(205, 220, 260, 305, 210),
    76: CreateBreakpoint(205, 220, 260, 305, 210),
    77: CreateBreakpoint(210, 225, 265, 310, 215),
    78: CreateBreakpoint(210, 225, 265, 310, 215),
    79: CreateBreakpoint(210, 225, 265, 310, 215),
    80: CreateBreakpoint(210, 225, 265, 310, 215),
    81: CreateBreakpoint(215, 230, 270, 315, 220),
    82: CreateBreakpoint(215, 230, 270, 315, 220),
    83: CreateBreakpoint(215, 230, 270, 315, 220),
    84: CreateBreakpoint(215, 230, 270, 315, 220),
    85: CreateBreakpoint(215, 230, 270, 315, 220),
    86: CreateBreakpoint(215, 230, 270, 315, 220),
    87: CreateBreakpoint(220, 235, 275, 320, 225),
    88: CreateBreakpoint(220, 235, 275, 320, 225),
    89: CreateBreakpoint(220, 235, 275, 320, 225),
    90: CreateBreakpoint(220, 235, 275, 320, 225),
    91: CreateBreakpoint(220, 235, 275, 320, 225),
    92: CreateBreakpoint(220, 235, 275, 320, 225),
    93: CreateBreakpoint(220, 235, 275, 320, 225),

    95: CreateBreakpoint(220, 235, 275, 320, 225),
    96: CreateBreakpoint(220, 235, 275, 320, 225),
    97: CreateBreakpoint(220, 235, 275, 320, 225),
    98: CreateBreakpoint(225, 240, 280, 325, 230),
    99: CreateBreakpoint(225, 237, 280, 325, 227),
    100: CreateBreakpoint(225, 238, 280, 325, 230),
    101: CreateBreakpoint(225, 240, 280, 325, 230),
    102: CreateBreakpoint(226, 241, 281, 326, 231),
    103: CreateBreakpoint(227, 242, 282, 327, 232),
    104: CreateBreakpoint(228, 243, 283, 328, 233),
    105: CreateBreakpoint(229, 244, 284, 329, 234),
    106: CreateBreakpoint(230, 245, 285, 330, 235),
    107: CreateBreakpoint(230, 245, 285, 330, 235),
    108: CreateBreakpoint(231, 246, 286, 331, 236),
    109: CreateBreakpoint(232, 247, 287, 332, 237),
    110: CreateBreakpoint(233, 248, 288, 333, 238),
    111: CreateBreakpoint(234, 249, 289, 334, 239),
    112: CreateBreakpoint(234, 249, 289, 334, 239),
    113: CreateBreakpoint(235, 250, 290, 335, 240),
    114: CreateBreakpoint(235, 250, 290, 335, 240),

    116: CreateBreakpoint(235, 250, 290, 335, 240),
    117: CreateBreakpoint(235, 250, 290, 335, 240),
    118: CreateBreakpoint(235, 250, 290, 335, 240),
    119: CreateBreakpoint(236, 251, 291, 336, 241),
    120: CreateBreakpoint(237, 252, 292, 337, 242),
    121: CreateBreakpoint(238, 253, 293, 338, 243),
    122: CreateBreakpoint(240, 255, 295, 340, 245),
    123: CreateBreakpoint(241, 256, 296, 341, 246),
    124: CreateBreakpoint(242, 257, 297, 342, 247),
    125: CreateBreakpoint(243, 258, 298, 343, 248),
    126: CreateBreakpoint(244, 259, 299, 344, 249),
    127: CreateBreakpoint(245, 260, 300, 345, 250),
    128: CreateBreakpoint(245, 260, 300, 345, 250),
    129: CreateBreakpoint(246, 261, 301, 346, 251),
    130: CreateBreakpoint(247, 262, 302, 347, 252),
    131: CreateBreakpoint(248, 263, 303, 348, 253),
    132: CreateBreakpoint(249, 264, 304, 349, 254),
    133: CreateBreakpoint(249, 264, 304, 349, 254),
    134: CreateBreakpoint(250, 265, 305, 350, 255),
    135: CreateBreakpoint(250, 266, 305, 350, 255),

    137: CreateBreakpoint(251, 266, 306, 351, 256),
    138: CreateBreakpoint(252, 267, 307, 352, 257),
    139: CreateBreakpoint(253, 268, 308, 353, 258),
    140: CreateBreakpoint(254, 269, 309, 354, 259),
    141: CreateBreakpoint(254, 269, 309, 354, 259),
    142: CreateBreakpoint(255, 270, 310, 355, 260),
    143: CreateBreakpoint(255, 270, 310, 355, 260),
}