using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SupabaseExporter;

public static class Sheets
{
    private static readonly GameData Lumina;
    
    public static readonly ExcelSheet<Map> MapSheet;
    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<Mount> MountSheet;
    public static readonly ExcelSheet<Treasure> TreasureSheet;
    public static readonly ExcelSheet<ClassJob> ClassJobSheet;
    public static readonly ExcelSheet<EventItem> EventItemSheet;
    public static readonly ExcelSheet<ExVersion> ExVersionSheet;
    public static readonly ExcelSheet<ContentType> ContentTypeSheet;
    public static readonly ExcelSheet<RetainerTask> RetainerTaskSheet;
    public static readonly ExcelSheet<GCSupplyDutyReward> GCSupplySheet;
    public static readonly ExcelSheet<TerritoryType> TerritoryTypeSheet;
    public static readonly ExcelSheet<NotoriousMonster> NotoriousMonsterSheet;
    public static readonly ExcelSheet<ItemUICategory> ItemUICategorySheet;
    public static readonly ExcelSheet<FateTokenType> FateTokenTypeSheet;
    
    public static readonly ExcelSheet<FishParameter> FishParameterSheet;
    public static readonly ExcelSheet<GatheringItem> GathererItemSheet;
    public static readonly ExcelSheet<SpearfishingItem> SpearfishingItemSheet;
    public static readonly ExcelSheet<GatheringPointBase> GatheringPointBaseSheet;
    public static readonly SubrowExcelSheet<GathererReductionReward> GathererReductionRewardSheet;

    public static readonly ExcelSheet<SubmarineExploration> SubmarineExplorationSheet;
    public static readonly ExcelSheet<SubmarineRank> SubmarineRankSheet;
    public static readonly ExcelSheet<SubmarinePart> SubmarinePartSheet;
    public static readonly ExcelSheet<SubmarineMap> SubmarineMapSheet;
    
    public static readonly SubrowExcelSheet<MapMarker> MapMarkerSheet;
    public static readonly ExcelSheet<HousingLandSet> HousingLandSetSheet;
    public static readonly SubrowExcelSheet<HousingMapMarkerInfo> HousingMapMarkerSheet;
    
    public static readonly ExcelSheet<World> WorldSheet;
    public static readonly ExcelSheet<WorldDCGroupType> WorldDCGroupSheet;
    
    public static readonly ExcelSheet<Fate> FateSheet;
    public static readonly ExcelSheet<DynamicEvent> DynamicEventSheet;
    
    public static readonly ExcelSheet<ENpcBase> ENpcBaseSheet;
    public static readonly ExcelSheet<Level> LevelSheet;
    
    // Other Languages
    public static readonly ExcelSheet<Item> ItemSheetFrench;
    public static readonly ExcelSheet<Item> ItemSheetGerman;
    public static readonly ExcelSheet<Item> ItemSheetJapanese;

    // Item
    public static readonly uint MaxItemId;
    public static readonly int EventItemCount;
    
    // Bnpc tracking
    public static readonly HashSet<uint> HousingTerritory;
    public static readonly HashSet<uint> RankedBnpcBase;
    
    public static readonly HashSet<uint> DisallowedBnpcBase = [
        0, // Retainer
        952, // Transporting Chocobo
        1008, // Eos
        3256, // Rook Autoturret
        6982, // Demi-Bahamut
        7245, // Earthly Star
        9037, // Happy Bunny
        9179, // Happy Bunny
        9181, // Happy Bunny
        9591, // Happy Bunny
        9610, // Happy Bunny
        9597, // Happy Bunny
        10055, // Happy Bunny
        10060, // Happy Bunny
        10064, // Happy Bunny
        10065, // Happy Bunny
        10487, // Seraph
        10897, // Bunshin
        11213, // Bunshin
        10489, // Esteem
        10490, // Queen Automaton
        13498, // Carbuncle
        13505, // Ruby Ifrit
        13506, // Emerald Garuda
        13507, // Topaz Titan
        13961, // Liturgic Bell
        14673, // Bishop Autoturret (PvP)
        16926, // Solar Bahamut
        18280, // Persistent Pot
        18281, // Persistent Pot
        18282, // Persistent Pot
        18287, // Persistent Pot
        18379, // Persistent Pot
        18868, // Feo Ul
        18869, // Feo Ul
    ];

    public static readonly Dictionary<string, uint> EngagementNames = new()
    {
        // Chinese
        { "腐烂蔬菜——皮里福尔", 1 },
        { "战争妖犬——恐惧妖犬", 2 },
        { "高火力陆战魔导兵器——守夜", 3 },
        { "新型飞行型魔导装甲——加百列", 4 },
        { "黑死鸟——阿克巴巴", 5 },
        { "怨念死灵——地生人", 6 },
        { "红陆行鸟之王——红色彗星", 7 },
        { "百兽之王——兽王莱昂", 8 },
        { "炎兽训练师——火焰百夫队", 9 },
        { "据点防卫魔导兵器——爱国者", 10 },
        { "邪眼妖兽——耶鲁", 11 },
        { "老练魔法师——铁胆狱火萨托瓦尔", 12 },
        { "钢铁魔兽——达因斯莱瓦", 13 },
        { "新型铁巨人——魔导劳工X式", 14 },
        { "战栗之角——奇尔维尼克", 15 },
        { "帝国湖岸堡攻城战", 16 },
        { "群蛇狂舞——妖战百人队", 17 },
        { "试制型飞行型魔导装甲——黑色燃焰", 18 },
        { "惨无人道的强化兵——超级调整兵达波格", 19 },
        { "复制告密者——第四军团谢米哈扎", 20 },
        { "焦土魔蝎——赫德提特", 21 },
        { "钢铁人马——魔导骑兵大队", 22 },
        { "第四军团军政官——梅内纽斯", 23 },
        { "异界魔王——汉比", 24 },
        { "邪恶冰狼——恶名苍狼", 25 },
        { "复制魔人——第四军团贝利亚斯", 26 },
        { "新兵器投入——机甲百人队", 27 },
        { "有角凶鸟——阿尔科诺斯特", 28 },
        { "复制统治者——第四军团哈修马利姆", 29 },
        { "光辉虹蛇——阿依达", 30 },
        { "战栗的百兽之王——兽王莱昂", 31 },
        { "旗舰达尔里阿达号攻略战", 32 },
        { "脑髓爱好者——夺心魔", 33 },
        { "黑色连队", 34 },
        { "愤怒的人造人——新月狂战士", 35 },
        { "潜影撕裂者——死亡爪", 36 },
        { "挣脱封印的大妖异——回廊恶魔", 37 },
        { "拟造使魔——水晶龙", 38 },
        { "双极的造物——神秘土偶", 39 },
        { "石制骑士团", 40 },
        { "传说中的鲨鱼——尼姆瓣齿鲨", 41 },
        { "双足狮人——跃立狮", 42 },
        { "防卫指令", 43 },
        { "厌鸟巨兽——进化加鲁拉", 44 },
        { "贩卖诅咒的商贩——金钱龟", 45 },
        { "城塞守卫——复原狮像", 46 },
        { "昏暗妖魂——鬼火苗", 47 },
        { "两歧塔 力之塔", 48 },
        { "四颚斧花——提蔛", 49 },
        { "魔女复制体——卡洛菲斯提莉二重身", 50 },
        { "纯白守护者——雪石膏之剑", 51 },
        { "禁书化形——古术魔典", 52 },
        { "暗红尸骸——赤龙", 53 },
        { "暴食咒鬼——阿尔戈尔", 54 },
        { "残暴的母蜘蛛——新月阿剌克涅", 55 },
        { "叛逆使魔——负隅宝石兽", 56 },
        { "天道好轮回——魔亡灵法师", 57 },
        { "求道的人造人——神木巨人", 58 },
        { "诅咒的继承者——惨白魔人", 59 },
        { "魔法军团——小小法师", 60 },
        { "孤岛的绑架犯——诱拐魔", 61 },
        { "苏醒的多头龙——魔许德拉", 62 },
        { "拟态使魔——变形法师", 63 },
        { "两岐塔 魔之塔", 64 },
        { "两歧塔 超魔之塔", 65 },
        
        // Korean
        { "썩은 야채 '피어리풀'", 1 },
        { "전쟁 요마견 '카니스 디루스'", 2 },
        { "고화력 육지전 마도 병기 '비질'", 3 },
        { "신형 비행 마도 아머 '가브리엘'", 4 },
        { "죽음의 검은 새 '아크바바'", 5 },
        { "원념의 사령 '스파르토이'", 6 },
        { "우두머리 초코보 '붉은 혜성'", 7 },
        { "백수의 왕 '마수왕 라이언'", 8 },
        { "화염마수조련사 '화염백인대'", 9 },
        { "거점 방위 병기 '패트리어트'", 10 },
        { "마안의 요수 '에알레'", 11 },
        { "노련한 마도사 '쇳불의 사르토부아르'", 12 },
        { "강철 마수 '다인슬라이프'", 13 },
        { "새로운 철거인 '마도 노동자 X형'", 14 },
        { "전율의 뿔 '칠레브니크'", 15 },
        { "카스트룸 라쿠스리토레 공성전", 16 },
        { "요귀술사 '요전백인대'", 17 },
        { "시험작 비행형 마도 아머 '블랙번'", 18 },
        { "비운의 강화병 '특수강화병 다보그'", 19 },
        { "모조 밀고자 'IV군단 셰미하자'", 20 },
        { "초토의 마수 전갈 '헤데테트'", 21 },
        { "강철의 인마 '마도기병대대'", 22 },
        { "제IV군단 분견대장 '메네니우스'", 23 },
        { "이계의 폐왕 '한비'", 24 },
        { "사악한 얼음늑대 '흐로드비트니르'", 25 },
        { "모조 마인 'IV군단 벨리아스'", 26 },
        { "신병기 투입 '기갑백인대'", 27 },
        { "뿔 달린 흉조 '알코노스트'", 28 },
        { "모조 통제자 'IV군단 하쉬말림'", 29 },
        { "빛나는 무지개뱀 '아이다'", 30 },
        { "전율하는 백수의 왕 '마수왕 라이언'", 31 },
        { "기함 달리아다 공략전", 32 },
        { "뇌수 애호가 '정신탈취자'", 33 },
        { "검은 연대", 34 },
        { "분노한 인조인간 '초승달 광전사'", 35 },
        { "은밀히 다가오는 손톱 '죽음손아귀'", 36 },
        { "봉인된 대요마 '회랑 악마'", 37 },
        { "모조된 존재 '수정룡'", 38 },
        { "수수께끼에 싸인 토우 '신비로운 우상'", 39 },
        { "돌로 만들어진 수호 기사들", 40 },
        { "전설의 상어 '니므 페탈로두스'", 41 },
        { "두 발로 선 사자 '직립 사자'", 42 },
        { "보안 명령", 43 },
        { "새를 싫어하는 거대 짐승 '네오 가루라'", 44 },
        { "저주를 파는 거북 '동전거북'", 45 },
        { "오래된 사자상 '복원된 사자'", 46 },
        { "어둑한 화톳불 '힝키펑크'", 47 },
        { "포크 타워: 힘의 탑", 48 },
        { "턱 네 개 마수 '펠레키스'", 49 },
        { "마녀의 복제 '칼로피스테리 분신'", 50 },
        { "하얀 수호자 '설화석고 검사'", 51 },
        { "금단의 마도서 '아르바텔'", 52 },
        { "검붉은 시룡 '루브룸 드래곤'", 53 },
        { "폭식의 저주괴물 '알골'", 54 },
        { "엽기적인 어미 거미 '초승달 아라크네'", 55 },
        { "반역의 사역마 '아틀라스 카벙클'", 56 },
        { "사령을 다루는 망령 '마기 강령술사'", 57 },
        { "수행하는 인조 인형 '엘름 기가스'", 58 },
        { "저주를 잇는 자 '창백한 마기아'", 59 },
        { "마도병단 '타이니 메이지'", 60 },
        { "외딴섬의 납치범 '유괴자'", 61 },
        { "각성한 다두룡 '마기 히드라'", 62 },
        { "변화하는 사역마 '메타몰퍼'", 63 },
        { "포크 타워: 마의 탑", 64 },
        { "극 포크 타워: 마의 탑", 65 },
    };
    
    static Sheets()
    {
        Lumina = new GameData(Environment.GetEnvironmentVariable("game_path")!);

        MapSheet = Lumina.GetExcelSheet<Map>()!;
        ItemSheet = Lumina.GetExcelSheet<Item>()!;
        MountSheet = Lumina.GetExcelSheet<Mount>()!;
        TreasureSheet = Lumina.GetExcelSheet<Treasure>()!;
        ClassJobSheet = Lumina.GetExcelSheet<ClassJob>()!;
        EventItemSheet = Lumina.GetExcelSheet<EventItem>()!;
        ExVersionSheet = Lumina.GetExcelSheet<ExVersion>()!;
        ContentTypeSheet = Lumina.GetExcelSheet<ContentType>()!;
        RetainerTaskSheet = Lumina.GetExcelSheet<RetainerTask>()!;
        GCSupplySheet = Lumina.GetExcelSheet<GCSupplyDutyReward>()!;
        TerritoryTypeSheet = Lumina.GetExcelSheet<TerritoryType>()!;
        NotoriousMonsterSheet = Lumina.GetExcelSheet<NotoriousMonster>()!;
        ItemUICategorySheet = Lumina.GetExcelSheet<ItemUICategory>()!;
        SubmarineExplorationSheet = Lumina.GetExcelSheet<SubmarineExploration>()!;
        SubmarineRankSheet = Lumina.GetExcelSheet<SubmarineRank>()!;
        SubmarinePartSheet = Lumina.GetExcelSheet<SubmarinePart>()!;
        SubmarineMapSheet = Lumina.GetExcelSheet<SubmarineMap>()!;
        MapMarkerSheet = Lumina.GetSubrowExcelSheet<MapMarker>()!;
        HousingLandSetSheet = Lumina.GetExcelSheet<HousingLandSet>()!;
        HousingMapMarkerSheet = Lumina.GetSubrowExcelSheet<HousingMapMarkerInfo>()!;
        WorldSheet = Lumina.GetExcelSheet<World>()!;
        WorldDCGroupSheet = Lumina.GetExcelSheet<WorldDCGroupType>()!;
        FateTokenTypeSheet = Lumina.GetExcelSheet<FateTokenType>()!;
        
        ENpcBaseSheet = Lumina.GetExcelSheet<ENpcBase>()!;
        LevelSheet = Lumina.GetExcelSheet<Level>()!;
        
        FateSheet = Lumina.GetExcelSheet<Fate>()!;
        DynamicEventSheet = Lumina.GetExcelSheet<DynamicEvent>()!;
        
        FishParameterSheet = Lumina.GetExcelSheet<FishParameter>()!;
        GathererItemSheet = Lumina.GetExcelSheet<GatheringItem>()!;
        SpearfishingItemSheet = Lumina.GetExcelSheet<SpearfishingItem>()!;
        GatheringPointBaseSheet = Lumina.GetExcelSheet<GatheringPointBase>()!;
        GathererReductionRewardSheet = Lumina.GetSubrowExcelSheet<GathererReductionReward>()!;
        
        ItemSheetFrench = Lumina.GetExcelSheet<Item>(Language.French)!;
        ItemSheetGerman = Lumina.GetExcelSheet<Item>(Language.German)!;
        ItemSheetJapanese = Lumina.GetExcelSheet<Item>(Language.Japanese)!;

        MaxItemId = ItemSheet.MaxBy(i => i.RowId).RowId;
        EventItemCount = EventItemSheet.Count;
        
        HousingTerritory = TerritoryTypeSheet.Where(r => r.TerritoryIntendedUse.RowId is 13 or 14).Select(r => r.RowId).ToHashSet();
        RankedBnpcBase = NotoriousMonsterSheet.Where(n => n.Rank is 1 or 2 or 3).Select(n => n.RowId).ToHashSet();

        foreach (var fate in Lumina.GetExcelSheet<DynamicEvent>(Language.English)!)
            EngagementNames[fate.Name.ToString()] = fate.RowId;
        
        foreach (var fate in Lumina.GetExcelSheet<DynamicEvent>(Language.German)!)
            EngagementNames[fate.Name.ToString()] = fate.RowId;
        
        foreach (var fate in Lumina.GetExcelSheet<DynamicEvent>(Language.French)!)
            EngagementNames[fate.Name.ToString()] = fate.RowId;
        
        foreach (var fate in Lumina.GetExcelSheet<DynamicEvent>(Language.Japanese)!)
            EngagementNames[fate.Name.ToString()] = fate.RowId;
    }
}
