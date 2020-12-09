using System;
using System.Collections.Generic;
using System.Text;

namespace VendingMachine
{
    public class Drink
    {
        public string m_name { get; set; }

        private Drink()
        {
            clear();
            return;
        }

        ~Drink()
        {
            clear();
        }

        private void clear()
        {
            m_name = "";
        }

        // インスタンスの生成
        public static int createInstance(out Drink drink, in String name)
        {
            int result = -1;

            // 返却値の初期化
            drink = null;

            try
            {
                if (name.Length <= 0)
                {
                    return result;
                }

                drink = new Drink();
                if (drink == null)
                {
                    return result;
                }

                drink.m_name = name;


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
                    drink = null;
                }
            }
            return result;
        }
    }
}
