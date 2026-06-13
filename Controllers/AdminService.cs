using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.DAL;
using SL_Bullion.Models;
using StackExchange.Redis;
using System.Reflection.Metadata;
using static StackExchange.Redis.Role;

namespace SL_Bullion.Controllers
{
    public class AdminService
    {
        private readonly BullionDbContext _context;
        private readonly ApplicationConstant _constatnt;

        public AdminService(BullionDbContext context, ApplicationConstant constatnt)
        {
            _context = context;
            _constatnt = constatnt;
        }
        internal async Task removeOrder(int id,string from,string to)
        {
            if (from == "open")
            {
                OpenOrder orderFrom = await _context.tblOpenOrder.FindAsync(id);
                if (to == "delete") {
                    sendOpenToDeleteOrder(orderFrom);
                }
                if (to == "open")
                {
                    sendOpenToOpenOrder(orderFrom);
                }
            }
            else if (from == "close")
            {
                CloseOrder orderFrom = await _context.tblCloseOrder.FindAsync(id);
                if (to == "delete")
                {
                    sendCloseToDeleteOrder(orderFrom);
                }
                else if(to=="open")
                {
                    sendCloseToOpenOrder(orderFrom);
                }
            }
            else if (from == "delete")
            {
                DeleteOrder orderFrom = await _context.tblDeleteOrder.FindAsync(id);
                if (to == "open")
                {
                    sendDeleteToOpenOrder(orderFrom);
                }
            }
            else if (from == "unfix")
            {
                UnFixOrder orderFrom = await _context.tblUnFixOrder.FindAsync(id);
                if (to == "delete")
                {
                    sendUnfixToDeleteOrder(orderFrom);
                }
            }
        }

        private void sendOpenToDeleteOrder(OpenOrder order)
        {
            DeleteOrder delete = new DeleteOrder
            {
                clientId = order.clientId,
                loginId = order.loginId,
                dealNo = order.dealNo,
                symbolId = order.symbolId,
                symbolName = order.symbolName,
                source = order.source,
                rateType = order.rateType,
                volume = order.volume,
                tradeType = order.tradeType,
                rate = order.rate,
                exchange = order.exchange,
                total = order.total,
                margin = order.margin,
                ip = order.ip,
                deviceType = order.deviceType,
                comment = order.comment,
                orderTime = order.orderTime,
                editorderTime = order.editorderTime,
                deleteTime = DateTime.Now
            };
            _context.tblDeleteOrder.Add(delete);
            _context.tblOpenOrder.Remove(order);
        }
        private void sendCloseToDeleteOrder(CloseOrder order)
        {
            DeleteOrder delete = new DeleteOrder
            {
                clientId = order.clientId,
                loginId = order.loginId,
                dealNo = order.dealNo,
                symbolId = order.symbolId,
                symbolName = order.symbolName,
                source = order.source,
                rateType = order.rateType,
                volume = order.volume,
                tradeType = order.tradeType,
                rate = order.rate,
                exchange = order.exchange,
                total = order.total,
                margin = order.margin,
                ip = order.ip,
                deviceType = order.deviceType,
                comment = order.comment,
                orderTime = order.orderTime,
                editorderTime = order.editorderTime,
                deleteTime = DateTime.Now
            };
            _context.tblDeleteOrder.Add(delete);
            _context.tblCloseOrder.Remove(order);
        }
        private void sendDeleteToOpenOrder(DeleteOrder order)
        {
            OpenOrder open = new OpenOrder
            {
                clientId = order.clientId,
                loginId = order.loginId,
                dealNo = order.dealNo,
                symbolId = order.symbolId,
                symbolName = order.symbolName,
                source = order.source,
                rateType = order.rateType,
                volume = order.volume,
                tradeType = order.tradeType,
                rate = order.rate,
                exchange = order.exchange,
                total = order.total,
                tax = _constatnt.getTaxRate(order.total, order.symbolId, order.clientId, order.loginId),
                margin = order.margin,
                ip = order.ip,
                deviceType = order.deviceType,
                comment = order.comment,
                orderTime = order.orderTime,
                editorderTime = order.editorderTime
            };
            _context.tblOpenOrder.Add(open);
            _context.tblDeleteOrder.Remove(order);
        }
        private void sendOpenToOpenOrder(OpenOrder order)
        {
            if (order != null)
            {
                order.tradeType = order.tradeType == 3 ? 1 : order.tradeType == 4 ? 2 : order.tradeType; ;
                _context.SaveChanges();
            }
        }
        private void sendCloseToOpenOrder(CloseOrder order)
        {
            OpenOrder open = new OpenOrder
            {
                clientId = order.clientId,
                loginId = order.loginId,
                dealNo = order.dealNo,
                symbolId = order.symbolId,
                symbolName = order.symbolName,
                source = order.source,
                rateType = order.rateType,
                volume = order.volume,
                tradeType = order.tradeType,
                rate = order.rate,
                exchange = order.exchange,
                total = order.total,
                tax = _constatnt.getTaxRate(order.total, order.symbolId, order.clientId, order.loginId),
                margin = order.margin,
                ip = order.ip,
                deviceType = order.deviceType,
                comment = order.comment,
                orderTime = order.orderTime,
                editorderTime = order.editorderTime
            };
            _context.tblOpenOrder.Add(open);
            _context.tblCloseOrder.Remove(order);
        }
        private void sendUnfixToDeleteOrder(UnFixOrder order)
        {
            DeleteOrder delete = new DeleteOrder
            {
                clientId = order.clientId,
                loginId = order.loginId,
                dealNo = order.dealNo,
                symbolId = order.symbolId,
                symbolName = order.symbolName,
                source = order.source,
                rateType = order.rateType,
                volume = order.volume,
                tradeType = order.tradeType,
                rate = order.rate,
                exchange = order.exchange,
                total = order.total,
                margin = order.margin,
                ip = order.ip,
                deviceType = order.deviceType,
                comment = order.comment,
                orderTime = order.orderTime,
                editorderTime = order.editorderTime,
                deleteTime = DateTime.Now
            };
            _context.tblDeleteOrder.Add(delete);
            _context.tblUnFixOrder.Remove(order);
        }
    }
}
