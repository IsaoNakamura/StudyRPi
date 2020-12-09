using System;

namespace VendingMachine
{
    class VendingMachine
    {
        static void Main(string[] args)
        {
            // 自動販売機を生成する
            VendingController controller = null;
            if (VendingController.createInstance(out controller) != 0)
            {
                return;
            }
            if (controller == null)
            {
                return;
            }

            // 自動販売機を整備する
            if (maintainMachine(ref controller) != 0)
            {
                return;
            }

            // 自動販売機を運用する
            if (controller.mainLoop() != 0)
            {
                return;
            }
        }

        // 自動販売機を整備する
        private static int maintainMachine(ref VendingController controller)
        {
            int result = -1;

            try
            {
                // 入力値チェック
                if (controller == null)
                {
                    return result;
                }

                // ドリンク入れる本数
                const int stock_num = 3;

                // 製品ラインナップ (ドリンク種類,価格)
                Tuple<string, int>[] lineup = {
                    new Tuple<string, int>("水", 100),
                    new Tuple<string, int>("コーラ", 150),
                    new Tuple<string, int>("お茶", 130)
                };

                // サポートする硬貨を
                int[] supportCoin = { 500, 100, 10, 50 };

                // サポートする紙幣を設定
                int[] supportBill = {};

                // サポートする硬貨を設定
                foreach(int coin in supportCoin)
                {
                    controller.m_supportCoin.Add(coin);
                }

                // サポートする紙幣を設定
                foreach (int bill in supportBill)
                {
                    controller.m_supportBill.Add(bill);
                }

                foreach (Tuple<string, int> element in lineup)
                {
                    // 価格テーブルにドリンク名と金額を追加
                    controller.m_dictDrinkPrice[element.Item1] = element.Item2;

                    // ドリンクストレージをメンテナンスのために取り出し
                    DrinkStorage drinkStorage = null;
                    if (controller.maintainStorage(out drinkStorage, element.Item1) != 0)
                    {
                        return result;
                    }
                    if (drinkStorage == null)
                    {
                        return result;
                    }

                    //ドリンクをストレージに追加
                    for (int i = 0; i < stock_num; i++)
                    {
                        Drink drink = null;
                        if (Drink.createInstance(out drink, element.Item1) != 0)
                        {
                            return result;
                        }

                        if (drinkStorage.pushDrink(ref drink) != 0)
                        {
                            return result;
                        }
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
