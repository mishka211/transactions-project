using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class TradeResultCounting
    {
        public ObservableCollection<UserAccount> UserAccounts { get; set; } = new ObservableCollection<UserAccount>();
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; } = new ObservableCollection<BrokerageAccount>();
        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();
        public ObservableCollection<Trade> Trades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<AccountBalance> AccountBalances { get; set; } = new ObservableCollection<AccountBalance>();
        public TradeResultCounting()
        {
            LoadInformationFromDatabease();
        }

        private void CountAccountBalanceWithTraderesult(double result, string date, int userAccountID)
        {
            AccountBalance accountBalance = new AccountBalance();
            for(int i = 0; i < AccountBalances.Count; i++)
            {
                if(AccountBalances[i].date == date && AccountBalances[i].accountID == userAccountID)
                {
                    accountBalance = AccountBalances[i];
                }
            }
            if (accountBalance.date == null)
            {
                accountBalance.accountID = userAccountID;
                accountBalance.date = date;

                using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"SELECT balance FROM AccountBalances WHERE accountID = @accountId AND date < @dealDate ORDER BY date DESC
                        LIMIT 1;", connection);
                    command.Parameters.AddWithValue("@accountId", userAccountID);
                    command.Parameters.AddWithValue("@dealDate", date);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            accountBalance = new AccountBalance
                            {
                                accountID = userAccountID,
                                date = date,
                                balance = Math.Round(Convert.ToDouble(reader["balance"]) + result, 2)
                            };
                        }
                    }
                }
                AddAccountBalanceToDatabase(accountBalance);
                AccountBalances.Add(accountBalance);
            }
            else
            {
                AccountBalances.Remove(accountBalance);
                accountBalance.balance = Math.Round(accountBalance.balance + result, 2);
                UpdateAccountBalanceInDatabase(accountBalance.balance, accountBalance.accountID, accountBalance.date);
                AccountBalances.Add(accountBalance);
            }
        }
        public TradeResult RecountTradeResult(int userAccountID, int number, double total)
        {
            Trade trade = new Trade();
            for(int i = 0; i < Trades.Count; i++)
            {
                if(Trades[i].userAccountID == userAccountID && Trades[i].number == number)
                {
                    trade = Trades[i];
                }
            }

            UserAccount userAccount = GetUserAccount(trade.userAccountID);
            BrokerageAccount brokerageAccount = GetBrokerageAccount(userAccount.brokerageAccountID);
            Instrument instrument = GetInstrument(trade.ticker);

            TradeResult tradeResult = GetTradeResult(trade, instrument, brokerageAccount);

            UpdateTradeResultToDataBase(tradeResult);
            UdateAccountBalanceWithTraderesult(tradeResult.total, trade.date, trade.userAccountID, total);
            return tradeResult;
        }

        private void UdateAccountBalanceWithTraderesult(double result, string date, int userAccountID, double oldResult)
        {
            double newBalance = 0;


            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(@"SELECT balance FROM AccountBalances WHERE accountID = @accountId AND date = @date;", connection);
                command.Parameters.AddWithValue("@accountId", userAccountID);
                command.Parameters.AddWithValue("@date", date);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var accountBalance = new AccountBalance
                        {
                            accountID = userAccountID,
                            date = date,
                            balance = Convert.ToDouble(reader["balance"])
                        };
                        AccountBalances.Remove(accountBalance);
                        newBalance = accountBalance.balance - oldResult + result;
                    }
                }
            }
            var newAccountBalance = new AccountBalance
            {
                accountID = userAccountID,
                date = date,
                balance = newBalance
            };
            AccountBalances.Add(newAccountBalance);
            UpdateAccountBalanceInDatabase(newBalance, userAccountID, date);
        }

        private TradeResult GetTradeResult(Trade trade, Instrument instrument, BrokerageAccount brokerageAccount)
        {
            double result = Math.Round(CalculateRes(trade, instrument), 2);
            double brokerCommission = Math.Round(CalculateBrokerCommission(trade, instrument, brokerageAccount), 2);
            double exchangeCommission = Math.Round(CalculateExchangeCommission(trade, instrument), 2);
            double total = result - brokerCommission - exchangeCommission;

            TradeResult tradeResult = new TradeResult
            {
                userAccountID = trade.userAccountID,
                number = trade.number,
                result = result,
                brokerCommission = brokerCommission,
                exchangeCommission = exchangeCommission,
                total = total
            };
            CountAccountBalanceWithTraderesult(tradeResult.total, trade.date, trade.userAccountID);
            return tradeResult;
        }

        private void LoadInformationFromDatabease()
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT accountID, accountName, userID, brokerageAccountID FROM UserAccounts", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserAccounts.Add(new UserAccount
                        {
                            accountID = reader.GetInt32(0),
                            accountName = reader.GetString(1),
                            userID = reader.GetInt32(2),
                            brokerageAccountID = reader.GetInt32(3)
                        });
                    }
                }
                var cmd1 = new SQLiteCommand("SELECT brokerageAccountID, brokerName, accountName, stockMinCommission, stockDefaultCommission, futuresMinCommission, futuresDefaultCommission FROM BrokerageAccounts", conn);
                using (var reader = cmd1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BrokerageAccounts.Add(new BrokerageAccount
                        {
                            brokerageAccountID = reader.GetInt32(0),
                            brokerName = reader.GetString(1),
                            accountName = reader.GetString(2),
                            stockMinCommission = reader.GetDouble(3),
                            stockDefaultCommission = reader.GetDouble(4),
                            futuresMinCommission = reader.GetDouble(5),
                            futuresDefaultCommission = reader.GetDouble(6)
                        });
                    }
                }
                var cmd2 = new SQLiteCommand("SELECT ticker, name, type, priceStep, priceStepCost, lotsize FROM Instruments", conn);
                using (var reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Instruments.Add(new Instrument
                        {
                            ticker = reader.GetString(0),
                            name = reader.GetString(1),
                            type = reader.GetString(2),
                            priceStep = reader.GetDouble(3),
                            priceStepCost = reader.GetDouble(4),
                            lotsize = reader.GetInt32(5)
                        });
                    }
                }
                var cmd3 = new SQLiteCommand("SELECT userAccountID, number, ticker, date, direction, openingPrice, closingPrice, lots FROM Trades", conn);
                using (var reader = cmd3.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Trades.Add(new Trade
                        {
                            userAccountID = reader.GetInt32(0),
                            number = reader.GetInt32(1),
                            ticker = reader.GetString(2),
                            date = reader.GetString(3),
                            direction = reader.GetString(4),
                            openingPrice = reader.GetDouble(5),
                            closingPrice = reader.GetDouble(6),
                            lots = reader.GetInt32(7)
                        });
                    }
                }
                var cmd4 = new SQLiteCommand("SELECT accountID, date, balance FROM AccountBalances", conn);
                using (var reader = cmd4.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AccountBalances.Add(new AccountBalance
                        {
                            accountID = reader.GetInt32(0),
                            date = reader.GetString(1),
                            balance = reader.GetDouble(2)
                        });
                    }
                }
            }
        }

        public TradeResult CalculateResult(Trade trade)
        {
            UserAccount userAccount = GetUserAccount(trade.userAccountID);
            BrokerageAccount brokerageAccount = GetBrokerageAccount(userAccount.brokerageAccountID);
            Instrument instrument = GetInstrument(trade.ticker);

            TradeResult tradeResult = GetTradeResult(trade, instrument, brokerageAccount);

            AddTradeResultToDatabase(tradeResult);
            return tradeResult;
        }

        private UserAccount GetUserAccount(int userAccountID)
        {
            UserAccount userAccount = new UserAccount();
            for (int i = 0; i < UserAccounts.Count; i++)
            {
                if (UserAccounts[i].accountID == userAccountID)
                {
                    userAccount = UserAccounts[i];
                }
            }
            return userAccount;
        }

        private BrokerageAccount GetBrokerageAccount(int brokerageAccountID)
        {
            BrokerageAccount brokerageAccount = new BrokerageAccount();
            for (int i = 0; i < BrokerageAccounts.Count; i++)
            {
                if (BrokerageAccounts[i].brokerageAccountID == brokerageAccountID)
                {
                    brokerageAccount = BrokerageAccounts[i];
                }
            }
            return brokerageAccount;
        }

        private Instrument GetInstrument(string ticker)
        {
            Instrument instrument = new Instrument();
            for (int i = 0; i < Instruments.Count; i++)
            {
                if (Instruments[i].ticker == ticker)
                {
                    instrument = Instruments[i];
                }
            }
            return instrument;
        }
        public double CalculateRes(Trade trade, Instrument instrument)
        {
            double result;
            if (trade.direction == "Лонг")
            {
                result = (trade.closingPrice - trade.openingPrice) / instrument.priceStep * instrument.priceStepCost * instrument.lotsize * trade.lots;
            }
            else
            {
                result = (trade.openingPrice - trade.closingPrice) / instrument.priceStep * instrument.priceStepCost * instrument.lotsize * trade.lots;
            }
            return result;
        }

        public double CalculateBrokerCommission(Trade trade, Instrument instrument, BrokerageAccount brokerageAccount)
        {
            double brokerCommission = 0;

            double minCommission1, minCommission2;
            if (instrument.type == "Акция")
            {
                minCommission1 = trade.openingPrice * brokerageAccount.stockDefaultCommission / 100;
                minCommission2 = trade.closingPrice * brokerageAccount.stockDefaultCommission / 100;
                if (brokerageAccount.stockMinCommission == 0)
                {
                    brokerCommission = minCommission1 + minCommission2;
                }
                else
                {
                    if(minCommission1 < brokerageAccount.stockMinCommission)
                    {
                        brokerCommission += brokerageAccount.stockMinCommission;
                    }
                    else
                    {
                        brokerCommission += minCommission1;
                    }

                    if (minCommission2 < brokerageAccount.stockMinCommission)
                    {
                        brokerCommission += brokerageAccount.stockMinCommission;
                    }
                    else
                    {
                        brokerCommission += minCommission2;
                    }
                }
            }
            else
            {
                if(trade.ticker == "CNY" || trade.ticker == "SI")
                {
                    minCommission1 = trade.openingPrice * brokerageAccount.futuresDefaultCommission / 100;
                    minCommission2 = trade.closingPrice * brokerageAccount.futuresDefaultCommission / 100;
                }
                else
                {
                    minCommission1 = trade.openingPrice * brokerageAccount.futuresDefaultCommission / 1000;
                    minCommission2 = trade.closingPrice * brokerageAccount.futuresDefaultCommission / 1000;
                }

                if (brokerageAccount.futuresMinCommission == 0)
                {
                    brokerCommission = minCommission1 + minCommission2;
                }
                else
                {
                    if (minCommission1 < brokerageAccount.futuresMinCommission)
                    {
                        brokerCommission += brokerageAccount.futuresMinCommission;
                    }
                    else
                    {
                        brokerCommission += minCommission1;
                    }

                    if (minCommission2 < brokerageAccount.futuresMinCommission)
                    {
                        brokerCommission += brokerageAccount.futuresMinCommission;
                    }
                    else
                    {
                        brokerCommission += minCommission2;
                    }
                }
            }

            return brokerCommission;
        }

        public double CalculateExchangeCommission(Trade trade, Instrument instrument)
        {
            double exchangeCommission = 0;

            if(instrument.type == "Акция")
            {
                double exCom1 = trade.openingPrice * 0.0003 * trade.lots * instrument.lotsize;
                double exCom2 = trade.closingPrice * 0.0003 * trade.lots * instrument.lotsize;
                if(exCom1 > 0.02)
                {
                    exchangeCommission += exCom1;
                }
                else
                {
                    exchangeCommission += 0.02;
                }

                if (exCom2 > 0.02)
                {
                    exchangeCommission += exCom2;
                }
                else
                {
                    exchangeCommission += 0.02;
                }
            }
            else
            {
                double exCom1, exCom2, percent;
                if(trade.ticker == "CNY" || trade.ticker == "SI")
                {
                    percent = 0.00462;
                }
                else if (trade.ticker == "MXI" || trade.ticker == "RTSM" || trade.ticker == "MIX" || trade.ticker == "RTS")
                {
                    percent = 0.0066;
                }
                else
                {
                    percent = 0.0198;
                }

                exCom1 = trade.openingPrice * percent / 100;
                exCom2 = trade.closingPrice * percent / 100;

                if(exCom1 > 0.01)
                {
                    exchangeCommission += exCom1 * trade.lots;
                }
                else
                {
                    exchangeCommission += 0.01 * trade.lots;
                }
                if (exCom2 > 0.01)
                {
                    exchangeCommission += exCom2 * trade.lots;
                }
                else
                {
                    exchangeCommission += 0.01 * trade.lots;
                }
            }
            return exchangeCommission;
        }






        public void AddTradeResultToDatabase(TradeResult tradeResult)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO TradesResults (userAccountID, number, result, brokerCommission, exchangeCommission, total) VALUES (@id, @n, @r, @bc, @ec, @t)", conn);
                cmd.Parameters.AddWithValue("@id", tradeResult.userAccountID);
                cmd.Parameters.AddWithValue("@n", tradeResult.number);
                cmd.Parameters.AddWithValue("@r", tradeResult.result);
                cmd.Parameters.AddWithValue("@bc", tradeResult.brokerCommission);
                cmd.Parameters.AddWithValue("@ec", tradeResult.exchangeCommission);
                cmd.Parameters.AddWithValue("@t", tradeResult.total);
                cmd.ExecuteNonQuery();
            }
        }
        private void UpdateTradeResultToDataBase(TradeResult tradeResult)
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();

                var command = new SQLiteCommand(@"
                UPDATE TradesResults SET 
                    result = @r,
                    brokerCommission = @bc,
                    exchangeCommission = @ec,
                    total = @t
                    WHERE userAccountID = @id AND number = @n", connection);

                command.Parameters.AddWithValue("@r", tradeResult.result);
                command.Parameters.AddWithValue("@bc", tradeResult.brokerCommission);
                command.Parameters.AddWithValue("@ec", tradeResult.exchangeCommission);
                command.Parameters.AddWithValue("@t", tradeResult.total);
                command.Parameters.AddWithValue("@id", tradeResult.userAccountID);
                command.Parameters.AddWithValue("@n", tradeResult.number);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteTradeResultToDatabase(int userAccountID, int number)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM TradesResults WHERE userAccountID = @id AND number = @n", conn);
                cmd.Parameters.AddWithValue("@id", userAccountID);
                cmd.Parameters.AddWithValue("@n", number);
                cmd.ExecuteNonQuery();
            }
        }
        private void AddAccountBalanceToDatabase(AccountBalance accountBalance)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO AccountBalances (accountID, date, balance) VALUES (@id, @d, @b)", conn);
                cmd.Parameters.AddWithValue("@id", accountBalance.accountID);
                cmd.Parameters.AddWithValue("@d", accountBalance.date);
                cmd.Parameters.AddWithValue("@b", accountBalance.balance);
                cmd.ExecuteNonQuery();
            }
        }
        private void UpdateAccountBalanceInDatabase(double balance, int accountID, string date)
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();

                var command = new SQLiteCommand(@"
                UPDATE AccountBalances SET 
                    balance = @b
                    WHERE accountID = @id AND date = @d", connection);

                command.Parameters.AddWithValue("@b", balance);
                command.Parameters.AddWithValue("@id", accountID);
                command.Parameters.AddWithValue("@d", date);
                command.ExecuteNonQuery();
            }
        }
    }
}
