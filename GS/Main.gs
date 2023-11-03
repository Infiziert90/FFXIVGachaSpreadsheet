function main() {
    cacheItemNames()

    coffer()
    bunny()

    setTime()
}

function coffer() {
    // Coffers
    getBoxData("Coffer", "Grand Company", 36635)
    getBoxData("Coffer", "Grand Company", 36636, 8)
    getBoxData("Coffer", "Venture", 32161)
    getBoxData("Coffer", "Sanctuary", 41667)

    // Lockboxes
    getBoxData("Bozja", "Bozja", 31357)
    getBoxData("Bozja", "Bozja", 33797, 8)
    getBoxData("Eureka", "Anemos", 22508)
    getBoxData("Eureka", "Pagos", 23142)
    getBoxData("Eureka", "Pagos", 23379, 8)
    getBoxData("Eureka", "Pyros", 24141)
    getBoxData("Eureka", "Pyros", 24142, 8)
    getBoxData("Eureka", "Hydatos", 24848)
    getBoxData("Eureka", "Hydatos", 24849, 8)

    // Deep Dungeons
    getBoxData("Deep Dungeon", "PotD", 16170, 4)
    getBoxData("Deep Dungeon", "PotD", 16171, 8)
    getBoxData("Deep Dungeon", "PotD", 16172, 12)
    getBoxData("Deep Dungeon", "PotD", 16173, 16)
    getBoxData("Deep Dungeon", "HoH", 23223, 4)
    getBoxData("Deep Dungeon", "HoH", 23224, 8)
    getBoxData("Deep Dungeon", "HoH", 23225, 12)
    getBoxData("Deep Dungeon", "EO", 38945, 4)
    getBoxData("Deep Dungeon", "EO", 38946, 8)
    getBoxData("Deep Dungeon", "EO", 38947, 12)
}

function bunny() {
    // Pagos
    getBunnyData("Bunny", "Bnuuy Pagos", 763, 2009532, 4)
    getBunnyData("Bunny", "Bnuuy Pagos", 763, 2009531, 8)
    getBunnyData("Bunny", "Bnuuy Pagos", 763, 2009530, 12)

    // Pyros
    getBunnyData("Bunny", "Bnuuy Pyros", 795, 2009532, 4)
    getBunnyData("Bunny", "Bnuuy Pyros", 795, 2009531, 8)
    getBunnyData("Bunny", "Bnuuy Pyros", 795, 2009530, 12)

    // Hydatos
    getBunnyData("Bunny", "Bnuuy Hydatos", 827, 2009532, 4)
    getBunnyData("Bunny", "Bnuuy Hydatos", 827, 2009531, 8)
    getBunnyData("Bunny", "Bnuuy Hydatos", 827, 2009530, 12)
}