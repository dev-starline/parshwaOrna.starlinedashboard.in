
using SL_Bullion.Models;

namespace SL_Bullion.Constant
{
    public class ResponseMessage
    {
        public string C101 = "User does not exist.";
        public string C102 = "Feedback details saved.";
        public string C103 = "Invalid date format.";
        public string C104 = "Data not available.";
        public string C105 = "User does not exist,Please register or contact to admin.";
        public string C106 = "User does not approove,Please contact to admin.";
        public string C107 = "User disable by admin.";
        public string C108 = "Username or password is incorrect.";
        public string C109 = "Register done,Please contact to admin for login.";
        public string C110 = "Trade disable by admin.";
        public string C111 = "Trade disable on symbol.";
        public string C112 = "Trade success.";
        public string C113 = "Stock not available on symbol.";
        public string C114 = "Margin not available for execute order.";
        public string C115 = "Order buy disable by admin.";
        public string C116 = "Order sell disable by admin.";
        public string C117 = "Pending order price within freezlevel.";
        public string C118 = "OffQuotes.";
        public string C119 = "Symbol session time expired.";
        public string C120 = "Oldpassword not match to current password.";
        public string C121 = "Password update successfully.";
        public string C122 = "Order not exist.";
        public string C123 = "Pendingorder update sucessfully.";
        public string C124 = "Order delete sucessfully.";
        public string C125 = "Mobile number already exist.";
        public string C126 = "Your account is verified via this otp.";
        public string C127 = "Your Otp is invalid ,plz try again with valid otp .";
        public string C128 = "Volume Not Between Min and Max Security Lot Size .";
        public string C129 = "OTR details saved.";
        public string C130 = "OTP Verified.";
        public string C131 = "Invalid OTP.";
        public string C132 = "Trade disable on coin.";
        public string C133 = "Account Is Not Active Please Contact To Admin";
        public string C134 = "Volume limit is exceeded.";
        public string C135 = "Symbol Trading Sesssion Time Is Closed";
        public string C136 = "Your trade has been successful.";
        public string C137 = "Coin stock is not available.";
        public string C138 = "User does not exist.";
        public string C139 = "Account Is expired Please Contact To Admin";
    }
    public class metaRequest
    {
        public string? symbol { get; set; }
        public double? volume { get; set; }
        public string? type { get; set; }
        public string? user { get; set; }
        public string comment { get; set; }
    }
    public class alertBody
    {
        public string? user { get; set; }
        public string? title { get; set; }
        public object? message { get; set; }
        public string? bit { get; set; }
    }
    public class PaginationViewModel<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

}
