using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace VendingMachine
{
    class VendingController
    {
        private Dictionary<String, DrinkStorage> m_dictDrinkStorage { get; set; }
        public Dictionary<String, int> m_dictDrinkPrice { get; private set; }

        public List<int> m_supportCoin { get; private set; }
        public List<int> m_supportBill { get; private set; }

        private VendingController()
        {
            clear();
            return;
        }

        ~VendingController()
        {
            clear();
        }

        private void clear()
        {
            if (m_dictDrinkStorage != null)
            {
                m_dictDrinkStorage.Clear();
                m_dictDrinkStorage = null;
            }

            if (m_dictDrinkPrice != null)
            {
                m_dictDrinkPrice.Clear();
                m_dictDrinkPrice = null;
            }

            if (m_supportCoin != null)
            {
                m_supportCoin.Clear();
                m_supportCoin = null;
            }

            if (m_supportBill != null)
            {
                m_supportBill.Clear();
                m_supportBill = null;
            }
        }

        // インスタンスの生成
        public static int createInstance(out VendingController controller)
        {
            int result = -1;

            // 返却値の初期化
            controller = null;

            try
            {
                controller = new VendingController();
                if (controller == null)
                {
                    return result;
                }

                controller.m_dictDrinkStorage = new Dictionary<String, DrinkStorage>();
                if (controller.m_dictDrinkStorage == null)
                {
                    return result;
                }

                controller.m_dictDrinkPrice = new Dictionary<String, int>();
                if (controller.m_dictDrinkPrice == null)
                {
                    return result;
                }

                controller.m_supportCoin = new List<int>();
                if (controller.m_supportCoin == null)
                {
                    return result;
                }

                controller.m_supportBill = new List<int>();
                if (controller.m_supportBill == null)
                {
                    return result;
                }

                // ここまでくれば正常終了
                result = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (result != 0)
                {
                    controller = null;
                }
            }
            return result;
        }

        // ドリンクストレージをメンテナンスする
        // ドリンクストレージにドリンクを入れる場合にのみ使用する
        public int maintainStorage(out DrinkStorage drinkStorage, in String drinkName)
        {
            int result = -1;

            // 返却値の初期化
            drinkStorage = null;

            try
            {
                if (m_dictDrinkStorage.ContainsKey(drinkName))
                {
                    // 存在する
                    drinkStorage = m_dictDrinkStorage[drinkName];
                }
                else
                {
                    // 存在しなければこの時点で生成
                    if (DrinkStorage.createInstance(out drinkStorage) != 0)
                    {
                        return result;
                    }
                    m_dictDrinkStorage.Add(drinkName, drinkStorage);
                }

                // ここまでくれば正常終了
                result = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (result != 0)
                {
                    drinkStorage = null;
                }
            }
            return result;
        }

        public int getStorage(out DrinkStorage drinkStorage, in String drinkName)
        {
            int result = -1;

            // 返却値の初期化
            drinkStorage = null;

            try
            {
                if (!m_dictDrinkStorage.ContainsKey(drinkName))
                {
                    // 存在しない
                    return result;

                }

                // 存在する
                drinkStorage = m_dictDrinkStorage[drinkName];


                // ここまでくれば正常終了
                result = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (result != 0)
                {
                    drinkStorage = null;
                }
            }
            return result;
        }

        private bool isEmptyAllStorage()
        {
            bool result = false;
            try
            {
                bool isALlEmpty = true;
                foreach (KeyValuePair<string, DrinkStorage> element in m_dictDrinkStorage)
                {
                    if(!element.Value.isEmpty())
                    {
                        isALlEmpty = false;
                        break;
                    }
                }

                result = isALlEmpty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return result;
        }

        private bool checkInputCoinType(in int input_amount)
        {
            bool result = false;
            try
            {
                if(m_supportCoin==null)
                {
                    return result;
                }

                if(m_supportCoin.Count <= 0)
                {
                    return result;
                }

                m_supportCoin.Sort();

                int minCoin = m_supportCoin.First();
                int maxCoin = m_supportCoin.Last();

                if (input_amount % minCoin != 0)
                {
                    return result;
                }

                result = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return result;
        }

        public int mainLoop()
        {
            int result = -1;
            int input_amount = 0;

            try
            {
                if (m_dictDrinkPrice == null)
                {
                    return result;
                }

                if (m_dictDrinkStorage == null)
                {
                    return result;
                }

                if(m_supportCoin == null)
                {
                    return result;
                }
                m_supportCoin.Sort();

                if (m_supportBill == null)
                {
                    return result;
                }
                m_supportBill.Sort();

                List<String> lineup = m_dictDrinkPrice.Keys.ToList();
                lineup.Sort();

                while (true)
                {
                    Console.WriteLine("いらっしゃいませ！");
                    Console.WriteLine("これらの飲み物があります。どれになさいますか？");

                    foreach (String drink_name in lineup)
                    {
                        Console.WriteLine("\t{0}:\t{1}", drink_name, m_dictDrinkPrice[drink_name]);
                    }


                    Console.WriteLine("「飲み物名:投入金額」の形式で入力ください。");


                    // 入力チェック
                    string input_line = Console.ReadLine();
                    string[] inputs = input_line.Split(':');
                    if (inputs.Length != 2)
                    {
                        Console.WriteLine("正しく「飲み物名:投入金額」の形式で入力ください。");
                        Console.WriteLine("\n");
                        continue;
                    }

                    String select_name = inputs[0];
                    input_amount = int.Parse(inputs[1]);

                    // 投入貨幣チェック
                    if (!checkInputCoinType(in input_amount))
                    {
                        Console.WriteLine("硬貨は10円, 50円, 100円, 500円のみ利用可能です。");
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        continue;
                    }

                    // 飲み物名チェック
                    if (!m_dictDrinkPrice.ContainsKey(select_name))
                    {
                        Console.WriteLine("「{0}」はありません。", select_name);
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        continue;
                    }

                    int drink_price = m_dictDrinkPrice[select_name];

                    // 投入額チェック
                    if (input_amount < drink_price)
                    {
                        Console.WriteLine("{0}が買えません。{1}円足りません。", select_name, drink_price-input_amount);
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        continue;
                    }


                    Console.WriteLine("あなたが選んだ飲み物「{0}」ですね。", select_name);
                    Console.WriteLine("あなたが投入した金額は「{0}」円ですね。", input_amount);

                    // ドリンクストレージにアクセス
                    DrinkStorage drinkStorage = null;
                    if (getStorage(out drinkStorage, select_name) != 0)
                    {
                        return result;
                    }

                    // 在庫チェック
                    if (drinkStorage.isEmpty())
                    {
                        Console.WriteLine("{0}は売り切れです。{1}円をお返しします。", select_name, input_amount);
                        Console.WriteLine("\n");
                        continue;
                    }

                    // ストレージからドリンクを出す
                    Drink drink = null;
                    if (drinkStorage.popDrink(out drink) != 0)
                    {
                        return result;
                    }

                    if (drink == null)
                    {
                        return result;
                    }

                    input_amount = input_amount - drink_price;

                    Console.WriteLine("{0}が買えました。お釣りは{1}円です。", drink.m_name, input_amount);
                    Console.WriteLine("\n");

                    if (isEmptyAllStorage())
                    {
                        Console.WriteLine("全ての商品は売り切れです。ありがとうございました。");
                        Console.WriteLine("\n");
                        break;
                    }
                }

                // ここまでくれば正常終了
                result = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (result != 0)
                {
                    Console.WriteLine("致命的なエラーが発生しました。");
                    Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                    Console.WriteLine("\n");
                }
            }
            return result;
        }

    }
}
