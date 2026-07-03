using Microsoft.EntityFrameworkCore;
using SL_Bullion.Models;

namespace SL_Bullion.DAL
{
    public class BullionDbContext:DbContext
    {
        public BullionDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<MasterLogin> tblMasterLogin { get; set; }
        public virtual DbSet<Master> tblMaster { get; set; }
        public virtual DbSet<ReferanceSymbol> tblReferanceSymbol { get; set; }
        public virtual DbSet<Symbol> tblSymbol { get; set; }
        public virtual DbSet<BankRate> tblBankRate { get; set; }
        public virtual DbSet<Contact> tblContact { get; set; }
        public virtual DbSet<Update> tblUpdate { get; set; }
        public virtual DbSet<Bank> tblBank { get; set; }
        public virtual DbSet<Feedback> tblFeedback { get; set; }
        public virtual DbSet<Group> tblGroup { get; set; }
        public virtual DbSet<GroupSymbol> tblGroupSymbol { get; set; }
        public virtual DbSet<Account> tblAccount { get; set; }
        public virtual DbSet<OpenOrder> tblOpenOrder { get; set; }
        public virtual DbSet<CloseOrder> tblCloseOrder { get; set; }
        public virtual DbSet<DeleteOrder> tblDeleteOrder { get; set; }
        public virtual DbSet<UnFixOrder> tblUnFixOrder { get; set; }
        public virtual DbSet<Otr> tblOtr { get; set; }
        public virtual DbSet<Coin> tblCoin { get; set; }
        public virtual DbSet<CoinBank> tblCoinBank { get; set; }
        public virtual DbSet<Hedge> tblHedge { get; set; }
        public virtual DbSet<HedgeSymbol> tblHedgeSymbol { get; set; }
        public virtual DbSet<SymbolSession> tblSymbolSession { get; set; }
        public virtual DbSet<BankLogo> tblBankLogo { get; set; }
        public virtual DbSet<Kyc> tblKyc { get; set; }
        public virtual DbSet<HedgeInfo> tblHedgeInfo { get; set; }
        public virtual DbSet<CoinInfo> tblCoinInfo { get; set; }
        public virtual DbSet<City> tblCity { get; set; }
        public virtual DbSet<OpenOrderCoin> tblOpenOrderCoin { get; set; } = default!;
        public virtual DbSet<OpenOrderCoinHistory> tblOpenOrderCoinHistory { get; set; } = default!;
        public virtual DbSet<CloseOrderCoin> tblCloseOrderCoin { get; set; } = default!;
        public virtual DbSet<SubCategory> tblSubCategory { get; set; }
        public virtual DbSet<Category> tblCategory { get; set; }
        public virtual DbSet<Jewellery> tblJewellery { get; set; }
        public virtual DbSet<OrderFailureLog> tblOrderFailureLog { get; set; }
        public virtual DbSet<Slider> tblSlider { get; set; }
    }
}
