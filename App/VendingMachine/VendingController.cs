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

        public int mainLoop()
        {
            int result = -1;

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


                List<String> lineup = m_dictDrinkPrice.Keys.ToList();
                lineup.OrderBy(val => val);

                while (true)
                {

                    Console.WriteLine("いらっしゃいませ！");
                    Console.WriteLine("これらの飲み物があります。どれになさいますか？");

                    foreach (String drink_name in lineup)
                    {
                        Console.WriteLine("\t{0}:\t{1}", drink_name, m_dictDrinkPrice[drink_name]);
                    }


                    int input_amount = 0;
                    String select_name = "";


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

                    select_name = inputs[0];
                    input_amount = int.Parse(inputs[1]);

                    // 投入貨幣チェック
                    if (input_amount % 10 != 0)
                    {
                        Console.WriteLine("硬貨10円, 50円, 100円, 500円のみ利用可能です。");
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
                        Console.WriteLine("致命的なエラーが発生しました。");
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        return result;
                    }

                    // 在庫チェック
                    if (drinkStorage.isEmpty())
                    {
                        Console.WriteLine("{0}は売り切れです。{1}円をお返しします。", select_name, input_amount);
                        Console.WriteLine("\n");
                        continue;
                    }

                    Drink drink = null;
                    if (drinkStorage.popDrink(out drink) != 0)
                    {
                        Console.WriteLine("致命的なエラーが発生しました。");
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        return result;
                    }

                    if (drink == null)
                    {
                        Console.WriteLine("致命的なエラーが発生しました。");
                        Console.WriteLine("投入した金額「{0}」円をお返しします。", input_amount);
                        Console.WriteLine("\n");
                        return result;
                    }

                    Console.WriteLine("{0}が買えました。お釣りは{1}円です。", drink.m_name, input_amount - drink_price);
                    Console.WriteLine("\n");

                    bool isALlEmpty = true;
                    foreach (String drink_name in lineup)
                    {
                        DrinkStorage drinkStorageWk = null;
                        if (getStorage(out drinkStorageWk, drink_name) != 0)
                        {
                            Console.WriteLine("致命的なエラーが発生しました。");
                            Console.WriteLine("\n");
                            return result;
                        }
                        if (!drinkStorageWk.isEmpty())
                        {
                            isALlEmpty = false;
                        }
                    }

                    if (isALlEmpty)
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
                }
            }
            return result;
        }

    }
}
