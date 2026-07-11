using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SL_Bullion.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblAccount",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    loginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firmName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    groupId = table.Column<int>(type: "int", nullable: false),
                    tradeAccess = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isRegister = table.Column<bool>(type: "bit", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    gst = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    margin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    mac = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAccount", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblBank",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    accountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ifscCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    branchName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bankLogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bankLogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBank", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblBankLogo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBankLogo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblBankRate",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    premiumGold = table.Column<double>(type: "float", nullable: false),
                    premiumSilver = table.Column<double>(type: "float", nullable: false),
                    spotTypeGold = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    spotTypeSilver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    interBankGold = table.Column<double>(type: "float", nullable: false),
                    interBankSilver = table.Column<double>(type: "float", nullable: false),
                    conversionGold = table.Column<double>(type: "float", nullable: false),
                    conversionSilver = table.Column<double>(type: "float", nullable: false),
                    customDutyGold = table.Column<double>(type: "float", nullable: false),
                    customDutySilver = table.Column<double>(type: "float", nullable: false),
                    marginGold = table.Column<double>(type: "float", nullable: false),
                    marginSilver = table.Column<double>(type: "float", nullable: false),
                    gstGold = table.Column<double>(type: "float", nullable: false),
                    gstSilver = table.Column<double>(type: "float", nullable: false),
                    divisionGold = table.Column<double>(type: "float", nullable: false),
                    divisionSilver = table.Column<double>(type: "float", nullable: false),
                    multiplyGold = table.Column<double>(type: "float", nullable: false),
                    multiplySilver = table.Column<double>(type: "float", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBankRate", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDisplay = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCloseOrder",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    loginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dealNo = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    symbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    volume = table.Column<double>(type: "float", nullable: false),
                    volumeOpen = table.Column<double>(type: "float", nullable: true),
                    tradeType = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<double>(type: "float", nullable: false),
                    rateOpen = table.Column<double>(type: "float", nullable: true),
                    exchange = table.Column<double>(type: "float", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    premium = table.Column<double>(type: "float", nullable: false),
                    margin = table.Column<double>(type: "float", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    orderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    editorderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    closeTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCloseOrder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblCloseOrderCoin",
                columns: table => new
                {
                    OpenOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DealNo = table.Column<long>(type: "bigint", nullable: false),
                    LoginID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SymbolID = table.Column<int>(type: "int", nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Exchange = table.Column<double>(type: "float", nullable: false),
                    Total = table.Column<double>(type: "float", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    OpenTradeDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TradeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TradeFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosePrice = table.Column<double>(type: "float", nullable: true),
                    CloseDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCloseOrderCoin", x => x.OpenOrderID);
                });

            migrationBuilder.CreateTable(
                name: "tblCoin",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isView = table.Column<bool>(type: "bit", nullable: false),
                    isStock = table.Column<bool>(type: "bit", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    buyPremium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sellPremium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    division = table.Column<double>(type: "float", nullable: false),
                    multiply = table.Column<double>(type: "float", nullable: false),
                    buyCommonPremium = table.Column<double>(type: "float", nullable: false),
                    sellCommonPremium = table.Column<double>(type: "float", nullable: false),
                    index = table.Column<int>(type: "int", nullable: false),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    coinStock = table.Column<double>(type: "float", nullable: false),
                    gst = table.Column<double>(type: "float", nullable: false),
                    coinId = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCoin", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblCoinBank",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    premiumGold = table.Column<double>(type: "float", nullable: false),
                    premiumSilver = table.Column<double>(type: "float", nullable: false),
                    spotTypeGold = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    spotTypeSilver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    interBankGold = table.Column<double>(type: "float", nullable: false),
                    interBankSilver = table.Column<double>(type: "float", nullable: false),
                    conversionGold = table.Column<double>(type: "float", nullable: false),
                    conversionSilver = table.Column<double>(type: "float", nullable: false),
                    customDutyGold = table.Column<double>(type: "float", nullable: false),
                    customDutySilver = table.Column<double>(type: "float", nullable: false),
                    marginGold = table.Column<double>(type: "float", nullable: false),
                    marginSilver = table.Column<double>(type: "float", nullable: false),
                    gstGold = table.Column<double>(type: "float", nullable: false),
                    gstSilver = table.Column<double>(type: "float", nullable: false),
                    divisionGold = table.Column<double>(type: "float", nullable: false),
                    divisionSilver = table.Column<double>(type: "float", nullable: false),
                    multiplyGold = table.Column<double>(type: "float", nullable: false),
                    multiplySilver = table.Column<double>(type: "float", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCoinBank", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblCoinInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCoinInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblContact",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    marqueeTop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    marqueeBottom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    whatsAppNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isBuy = table.Column<bool>(type: "bit", nullable: false),
                    isSell = table.Column<bool>(type: "bit", nullable: false),
                    isHigh = table.Column<bool>(type: "bit", nullable: false),
                    isLow = table.Column<bool>(type: "bit", nullable: false),
                    isRate = table.Column<bool>(type: "bit", nullable: false),
                    isCoinRate = table.Column<bool>(type: "bit", nullable: false),
                    isTrade = table.Column<bool>(type: "bit", nullable: false),
                    isLogin = table.Column<bool>(type: "bit", nullable: false),
                    isHedge = table.Column<bool>(type: "bit", nullable: false),
                    bannerWeb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bannerApp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goldDifferance = table.Column<int>(type: "int", nullable: false),
                    silverDifferance = table.Column<int>(type: "int", nullable: false),
                    freezOuter = table.Column<int>(type: "int", nullable: false),
                    freezInner = table.Column<int>(type: "int", nullable: false),
                    offQuotes = table.Column<int>(type: "int", nullable: false),
                    plDelete = table.Column<TimeOnly>(type: "time", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isCoinTrade = table.Column<bool>(type: "bit", nullable: false),
                    goldCoinHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    silverCoinHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isGoldCoinHeader = table.Column<bool>(type: "bit", nullable: false),
                    isSilverCoinHeader = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblContact", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblDeleteOrder",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    loginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dealNo = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    symbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    volume = table.Column<double>(type: "float", nullable: false),
                    tradeType = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<double>(type: "float", nullable: false),
                    exchange = table.Column<double>(type: "float", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    premium = table.Column<double>(type: "float", nullable: false),
                    margin = table.Column<double>(type: "float", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    orderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    editorderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleteTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDeleteOrder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblFeedback",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblFeedback", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblGroup",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    buyPremiumGold = table.Column<double>(type: "float", nullable: false),
                    sellPremiumGold = table.Column<double>(type: "float", nullable: false),
                    buyPremiumSilver = table.Column<double>(type: "float", nullable: false),
                    sellPremiumSilver = table.Column<double>(type: "float", nullable: false),
                    isTrade = table.Column<bool>(type: "bit", nullable: false),
                    isEnable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblGroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblGroupSymbol",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    groupId = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    isView = table.Column<bool>(type: "bit", nullable: false),
                    buyPremium = table.Column<double>(type: "float", nullable: false),
                    sellPremium = table.Column<double>(type: "float", nullable: false),
                    oneClick = table.Column<double>(type: "float", nullable: false),
                    inTotal = table.Column<double>(type: "float", nullable: false),
                    step = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblGroupSymbol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblHedge",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    hedgeSymbolId = table.Column<int>(type: "int", nullable: false),
                    division = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblHedge", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblHedgeInfo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    request = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblHedgeInfo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblHedgeSymbol",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblHedgeSymbol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblJewellery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    SubCategoryId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    isDisplay = table.Column<bool>(type: "bit", nullable: false),
                    TagNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblJewellery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblKyc",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    companyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    companyAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    partnerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    partnerMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    officeMobile1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    officeMobile2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    residenceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ifsc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gstNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    panNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblKyc", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblMaster",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    firmName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    clientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    symbol = table.Column<int>(type: "int", nullable: false),
                    group = table.Column<int>(type: "int", nullable: false),
                    versionAndroid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    versionIos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passwordFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    startLoginId = table.Column<int>(type: "int", nullable: false),
                    startDealNo = table.Column<int>(type: "int", nullable: false),
                    isMeta = table.Column<bool>(type: "bit", nullable: false),
                    isEndUserLogin = table.Column<bool>(type: "bit", nullable: false),
                    isOtp = table.Column<bool>(type: "bit", nullable: false),
                    isCoin = table.Column<bool>(type: "bit", nullable: false),
                    isJewellery = table.Column<bool>(type: "bit", nullable: false),
                    isKyc = table.Column<bool>(type: "bit", nullable: false),
                    isOtr = table.Column<bool>(type: "bit", nullable: false),
                    isCity = table.Column<bool>(type: "bit", nullable: false),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    lastCoinDealNo = table.Column<int>(type: "int", nullable: false),
                    coinTradeOn = table.Column<bool>(type: "bit", nullable: false),
                    coinStartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    coinEndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isCoinTrade = table.Column<bool>(type: "bit", nullable: false),
                    isCategory = table.Column<bool>(type: "bit", nullable: false),
                    totalSlider = table.Column<int>(type: "int", nullable: true),
                    isSlider = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblMaster", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblMasterLogin",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblMasterLogin", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblOpenOrder",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    loginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dealNo = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    symbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    volume = table.Column<double>(type: "float", nullable: false),
                    tradeType = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<double>(type: "float", nullable: false),
                    exchange = table.Column<double>(type: "float", nullable: false),
                    differenceRate = table.Column<double>(type: "float", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    premium = table.Column<double>(type: "float", nullable: false),
                    premiumLimit = table.Column<double>(type: "float", nullable: false),
                    margin = table.Column<double>(type: "float", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isLimit = table.Column<bool>(type: "bit", nullable: false),
                    isHedge = table.Column<bool>(type: "bit", nullable: false),
                    orderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    editorderTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOpenOrder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblOpenOrderCoin",
                columns: table => new
                {
                    OpenOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DealNo = table.Column<long>(type: "bigint", nullable: false),
                    LoginID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SymbolID = table.Column<int>(type: "int", nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Exchange = table.Column<double>(type: "float", nullable: false),
                    Total = table.Column<double>(type: "float", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    OpenTradeDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TradeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TradeFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOpenOrderCoin", x => x.OpenOrderID);
                });

            migrationBuilder.CreateTable(
                name: "tblOpenOrderCoinHistory",
                columns: table => new
                {
                    OpenOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DealNo = table.Column<long>(type: "bigint", nullable: false),
                    LoginID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SymbolID = table.Column<int>(type: "int", nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Exchange = table.Column<double>(type: "float", nullable: false),
                    Total = table.Column<double>(type: "float", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    OpenTradeDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TradeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TradeFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosePrice = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOpenOrderCoinHistory", x => x.OpenOrderID);
                });

            migrationBuilder.CreateTable(
                name: "tblOtr",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    firmname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    otp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isOtpVerify = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOtr", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblReferanceSymbol",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isMaster = table.Column<bool>(type: "bit", nullable: false),
                    isView = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblReferanceSymbol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblSlider",
                columns: table => new
                {
                    SliderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    SliderThumbnailPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SliderPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSlider", x => x.SliderId);
                });

            migrationBuilder.CreateTable(
                name: "tblSubCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDisplay = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSubCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblSymbol",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    symbolType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isView = table.Column<bool>(type: "bit", nullable: false),
                    isTerminal = table.Column<bool>(type: "bit", nullable: false),
                    isTrade = table.Column<bool>(type: "bit", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    buyPremium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sellPremium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    division = table.Column<double>(type: "float", nullable: false),
                    multiply = table.Column<double>(type: "float", nullable: false),
                    gst = table.Column<double>(type: "float", nullable: false),
                    buyCommonPremium = table.Column<double>(type: "float", nullable: false),
                    sellCommonPremium = table.Column<double>(type: "float", nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false),
                    high = table.Column<int>(type: "int", nullable: false),
                    low = table.Column<int>(type: "int", nullable: false),
                    useStock = table.Column<int>(type: "int", nullable: false),
                    initialMargin = table.Column<int>(type: "int", nullable: false),
                    index = table.Column<int>(type: "int", nullable: false),
                    digit = table.Column<int>(type: "int", nullable: false),
                    isBill = table.Column<bool>(type: "bit", nullable: false),
                    isComment = table.Column<bool>(type: "bit", nullable: false),
                    gstBill = table.Column<double>(type: "float", nullable: false),
                    tcsBill = table.Column<double>(type: "float", nullable: false),
                    tdsBill = table.Column<double>(type: "float", nullable: false),
                    createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    changePremiumDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    rateDisplayProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSymbol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblSymbolSession",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    session = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSymbolSession", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblUnFixOrder",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    loginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dealNo = table.Column<int>(type: "int", nullable: false),
                    symbolId = table.Column<int>(type: "int", nullable: false),
                    symbolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    volume = table.Column<double>(type: "float", nullable: false),
                    tradeType = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<double>(type: "float", nullable: false),
                    exchange = table.Column<double>(type: "float", nullable: false),
                    differenceRate = table.Column<double>(type: "float", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    premium = table.Column<double>(type: "float", nullable: false),
                    premiumLimit = table.Column<double>(type: "float", nullable: false),
                    margin = table.Column<double>(type: "float", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isLimit = table.Column<bool>(type: "bit", nullable: false),
                    isHedge = table.Column<bool>(type: "bit", nullable: false),
                    orderTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    editorderTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUnFixOrder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tblUpdate",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientId = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUpdate", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblAccount");

            migrationBuilder.DropTable(
                name: "tblBank");

            migrationBuilder.DropTable(
                name: "tblBankLogo");

            migrationBuilder.DropTable(
                name: "tblBankRate");

            migrationBuilder.DropTable(
                name: "tblCategory");

            migrationBuilder.DropTable(
                name: "tblCity");

            migrationBuilder.DropTable(
                name: "tblCloseOrder");

            migrationBuilder.DropTable(
                name: "tblCloseOrderCoin");

            migrationBuilder.DropTable(
                name: "tblCoin");

            migrationBuilder.DropTable(
                name: "tblCoinBank");

            migrationBuilder.DropTable(
                name: "tblCoinInfo");

            migrationBuilder.DropTable(
                name: "tblContact");

            migrationBuilder.DropTable(
                name: "tblDeleteOrder");

            migrationBuilder.DropTable(
                name: "tblFeedback");

            migrationBuilder.DropTable(
                name: "tblGroup");

            migrationBuilder.DropTable(
                name: "tblGroupSymbol");

            migrationBuilder.DropTable(
                name: "tblHedge");

            migrationBuilder.DropTable(
                name: "tblHedgeInfo");

            migrationBuilder.DropTable(
                name: "tblHedgeSymbol");

            migrationBuilder.DropTable(
                name: "tblJewellery");

            migrationBuilder.DropTable(
                name: "tblKyc");

            migrationBuilder.DropTable(
                name: "tblMaster");

            migrationBuilder.DropTable(
                name: "tblMasterLogin");

            migrationBuilder.DropTable(
                name: "tblOpenOrder");

            migrationBuilder.DropTable(
                name: "tblOpenOrderCoin");

            migrationBuilder.DropTable(
                name: "tblOpenOrderCoinHistory");

            migrationBuilder.DropTable(
                name: "tblOtr");

            migrationBuilder.DropTable(
                name: "tblReferanceSymbol");

            migrationBuilder.DropTable(
                name: "tblSlider");

            migrationBuilder.DropTable(
                name: "tblSubCategory");

            migrationBuilder.DropTable(
                name: "tblSymbol");

            migrationBuilder.DropTable(
                name: "tblSymbolSession");

            migrationBuilder.DropTable(
                name: "tblUnFixOrder");

            migrationBuilder.DropTable(
                name: "tblUpdate");
        }
    }
}
