﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTrade
{
    public class Position
    {
        public enum PositionState
        {
              NONE
            , LONG
            , SHORT
        };

        public enum OrderState
        {
             NONE
           , ORDERED
           , ACTIVE
           , COMPLETED
           , CANCELED
           , REJECTED
        };

        public enum StrategyType
        {
              UNKNOWN
            , SCAM
            , CROSS_EMA
            , SWING
            , DOTEN
            , HIGE
            , FLONT_LINE
        };

        private PositionState state { get; set; }

        private OrderState entry_state { get; set; }
        public string entry_id { get; private set; }
        public double entry_price { get; set; }
        public string entry_date { get; set; }
        public string entry_parent_id { get; set; }
        public int entry_page_id { get; set; }

        private OrderState exit_state { get; set; }
        public string exit_id { get; set; }
        public double exit_price { get; set; }
        public string exit_date { get; set; }

        public StrategyType strategy_type { get; set; }

        public double limit_buy_price { get; set; }
        public double limit_sell_price { get; set; }

        public List<string> entry_child_ids { get; set; }

        public Position()
        {
            init();
            return;
        }

        public void init()
        {
            state = PositionState.NONE;

            entry_state = OrderState.NONE;
            entry_id = "";
            entry_price = 0.0;
            entry_date = "";


            exit_state = OrderState.NONE;
            exit_id = "";
            exit_price = 0.0;
            exit_date = "";

            strategy_type = StrategyType.UNKNOWN;

            return;
        }

        public bool isOrderNone()
        {
            if (entry_state == OrderState.NONE && exit_state == OrderState.NONE)
            {
                return true;
            }
            return false;
        }

        public bool isEntryNone()
        {
            if (entry_state == OrderState.NONE)
            {
                return true;
            }
            return false;
        }

        public bool isEntryOrdered()
        {
            if (entry_state == OrderState.ORDERED)
            {
                return true;
            }
            return false;
        }

        public bool isEntryActive()
        {
            if (entry_state == OrderState.ACTIVE)
            {
                return true;
            }
            return false;
        }

        public bool isEntryCompleted()
        {
            if (entry_state == OrderState.COMPLETED)
            {
                return true;
            }
            return false;
        }

        public bool isExitNone()
        {
            if (exit_state == OrderState.NONE)
            {
                return true;
            }
            return false;
        }

        public bool isExitOrdered()
        {
            if (exit_state == OrderState.ORDERED)
            {
                return true;
            }
            return false;
        }

        public bool isExitActive()
        {
            if (exit_state == OrderState.ACTIVE)
            {
                return true;
            }
            return false;
        }

        public bool isExitCompleted()
        {
            if (exit_state == OrderState.COMPLETED)
            {
                return true;
            }
            return false;
        }

        public string getOrderStateStr()
        {
            string result = "NONE";

            if (isExitActive())
            {
                result = "ACTIVE";
            }
            else if (isExitCompleted())
            {
                result = "COMPLETED";
            }

            return result;
        }

        public string getPositionStateStr()
        {
            string result = "NONE";

            string strategy_str = "";
            if (strategy_type == StrategyType.SCAM)
            {
                strategy_str = "(SCAM)";
            }
            else if (strategy_type == StrategyType.SWING)
            {
                strategy_str = "(SWING)";
            }
            else if (strategy_type == StrategyType.DOTEN)
            {
                strategy_str = "(DOTEN)";
            }

            if (isLong())
            {
                result = "LONG" + strategy_str;
            }
            else if (isShort())
            {
                result = "SHORT" + strategy_str;
            }

            return result;
        }

        public bool isNone()
        {
            if (state == PositionState.NONE)
            {
                return true;
            }
            return false;
        }

        public bool isLong()
        {
            if (state == PositionState.LONG)
            {
                return true;
            }
            return false;
        }

        public bool isShort()
        {
            if (state == PositionState.SHORT)
            {
                return true;
            }
            return false;
        }

        // LONGエントリー注文
        // 成行注文に使用する
        public void entryLongOrder(string _acceptance_id, string _entry_date)
        {
            entry_id = _acceptance_id;
            entry_state = OrderState.ACTIVE;
            state = PositionState.LONG;
            entry_date = _entry_date;
            return;
        }

        // SHORTエントリー注文
        // 成行注文に使用する
        public void entryShortOrder(string _acceptance_id, string _entry_date)
        {
            entry_id = _acceptance_id;
            entry_state = OrderState.ACTIVE;
            state = PositionState.SHORT;
            entry_date = _entry_date;
            return;
        }

        // エントリー注文
        // 成行注文以外に使用する
        public void entryOrder(string _acceptance_id, string _entry_date)
        {
            entry_id = _acceptance_id;
            entry_state = OrderState.ORDERED;
            state = PositionState.NONE;
            entry_date = _entry_date;
            return;
        }

        // エントリー注文したものが注文板にのる
        // 成行注文以外に使用する
        public void entryActive()
        {
            entry_state = OrderState.ACTIVE;
            return;
        }


        public void exitOrder(string _acceptance_id, string _exit_date)
        {
            exit_id = _acceptance_id;
            exit_state = OrderState.ACTIVE;
            exit_date = _exit_date;
        }

        public void exitActive()
        {
            exit_state = OrderState.ACTIVE;
            return;
        }

        public void entry(double _entry_price)
        {
            entry_price = _entry_price;
            entry_state = OrderState.COMPLETED;
            return;
        }

        public void cancel()
        {
            entry_state = OrderState.CANCELED;
            state = PositionState.NONE;
            return;
        }

        public void entry(double _entry_price, string _side)
        {
            if (_side == "BUY")
            {
                state = PositionState.LONG;
            }
            else if (_side == "SELL")
            {
                state = PositionState.SHORT;
            }
            else
            {
                state = PositionState.NONE;
            }
            entry_price = _entry_price;
            entry_state = OrderState.COMPLETED;
            return;
        }

        public void exit(double _exit_price)
        {
            exit_price = _exit_price;
            exit_state = OrderState.COMPLETED;
            return;
        }

        public void reject()
        {
            return;
        }

        public void entryRecject()
        {
            entry_state = OrderState.REJECTED;
        }

        public double calcProfit(double now_price)
        {
            double result = 0.0;

            if (entry_state != OrderState.COMPLETED)
            {
                result = 0.0;
                return result;
            }

            if (isLong())
            {
                result = now_price - entry_price;
            }
            else if (isShort())
            {
                result = entry_price - now_price;
            }

            return result;
        }

        public double getProfit()
        {
            double result = 0.0;

            if (exit_state != OrderState.COMPLETED)
            {
                result = 0.0;
                return result;
            }

            if (isLong())
            {
                result = exit_price - entry_price;
            }
            else if (isShort())
            {
                result = entry_price - exit_price;
            }

            return result;
        }
    }
}
