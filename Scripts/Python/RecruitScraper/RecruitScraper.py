# Chromeドライバー(selenium)を取得
def getChromeDriver():
    import requests
    from selenium import webdriver
    from webdriver_manager.chrome import ChromeDriverManager
    from webdriver_manager.utils import chrome_version
    version = chrome_version()
    url_chromedriver = 'http://chromedriver.storage.googleapis.com/LATEST_RELEASE_' + version
    response = requests.get(url_chromedriver)
    driver = webdriver.Chrome(executable_path=ChromeDriverManager(response.text).install())
    return driver

# ワードリストから指定ワードのインデックスを取得
def getWordIndex(word, word_list):
    idx = -1
    if( word in word_list):
        idx = word_list.index(word)
    return idx

# メイン処理
if __name__ == '__main__':
    import time
    import sys

    accountId = sys.argv[1]
    password = sys.argv[2]
    url="https://pdt.r-agent.com/pdt/app/pdt_login_view"

    driver = getChromeDriver()
    driver.get(url)
    time.sleep(5)

    # ログイン画面でアカウントとパスワードを入力してログインボタンをクリックする
    acount_box = driver.find_element_by_name("accountId")
    password_box = driver.find_element_by_name("password")
    login_button = driver.find_element_by_name("loginButton")
    acount_box.send_keys(accountId)
    password_box.send_keys(password)
    time.sleep(1)
    login_button.click()
    time.sleep(1)

    # 求人構造体リストを取得
    job_list = driver.find_elements_by_class_name("jobOfferPost-job")

    # 求人属性種別タプル
    header_word_list = ( '勤務地', '就業時間', '想定年収', '従業員数','仕事の内容','必要な能力・経験' )

    # 求人辞書(key=会社名 value=求人詳細辞書(key=求人種別, value=求人種別に応じた値) )
    job_dict = {}

    # 求人構造体全てをループ
    for job in job_list:
        # 会社名構造体を取得
        company = job.find_element_by_class_name("jobOfferPost-jobSubTitle")
        # 求人詳細構造体を取得
        details = job.find_element_by_class_name("jobOfferPost-jobDetails")

        # 求人詳細リスト
        #  求人詳細文字列を改行で分割する
        word_list = details.text.split('\n')
        
        # 求人詳細リスト上インデックスリスト
        # 　求人種別が求人詳細リストのどこにあるかわかるインデックスリスト
        header_idx_list = []

        # 求人種別のインデックスと求人種別名の対応辞書(key=求人詳細リスト上インデックス, value=求人種別)
        header_dict = {}
        
        for header_word in header_word_list:
            header_idx = getWordIndex(header_word, word_list)
            if(header_idx!=-1):
                header_idx_list.append(header_idx)
                header_dict[header_idx] = header_word

        # インデックスリストを昇順にソート
        header_idx_list.sort()

        # 求人詳細辞書((key=求人種別, value=求人種別に応じた値)
        detail_dict = {}

        # 求人詳細リストを解析して求人辞書に格納する求人詳細辞書を構築
        curr_idx = 0
        while curr_idx < len(header_idx_list):
            # 求人種別currと求人種別nextの間にあるものが求人種別currの求人詳細値
            next_idx = curr_idx+1
            curr_header_idx = header_idx_list[curr_idx]
            beg_word_idx = curr_header_idx + 1

            curr_detail = ''

            if( next_idx >= len(header_idx_list) ):
                curr_word_list = word_list[beg_word_idx]
                curr_detail = ''.join(curr_word_list)
            else:
                end_word_idx = header_idx_list[next_idx]
                curr_word_list = word_list[beg_word_idx:end_word_idx]
                curr_detail = ''.join(curr_word_list)

            curr_header = header_dict[curr_header_idx]
            detail_dict[curr_header]=curr_detail
            curr_idx+=1
        # 求人辞書に追加
        job_dict[company.text] = detail_dict

    driver.quit()

    # 結果をファイルに出力する
    fout = open('JobList.txt','wt')

    print('#会社名', file=fout, sep='', end='\t')
    for header_word in header_word_list:
        print(header_word, file=fout, sep='', end='\t')
    print('', file=fout, sep='', end='\n')

    company_list = job_dict.keys()
    for company in company_list:
        print(company, file=fout, sep='', end='\t')
        detail_dict = job_dict[company]
        for header_word in header_word_list:
            detail = 'none'
            if( header_word in detail_dict):
                detail = detail_dict[header_word]
            print(detail, file=fout, sep='', end='\t')
        print('', file=fout, sep='', end='\n')
    fout.close()

# 参考：格納状態
# <input type="text" name="accountId" value="" autocomplete="off" class="mod-textbox">
# <input type="password" name="password" value="" class="mod-textbox">
# <input type="submit" name="loginButton" value="ログイン" class="mod-button-login">
# <div class="jobOfferPost-jobList" data-jobofferpost-ui="jobList">
# <h3 class="jobOfferPost-jobSubTitle">株式会社</h3>
# <div class="jobOfferPost-jobDetailsContainer"><h3 class="jobOfferPost-jobDetails__content">仕事の内容</h3>
# <div class="jobOfferPost-jobDetailsContainer"><h3 class="jobOfferPost-jobDetails__ability">必要な能力・経験</h3>
# <dt class="jobOfferPost-jobDetails__officeAreas">勤務地</dt>
# <dt class="jobOfferPost-jobDetails__salary">想定年収</dt>
# <dt class="jobOfferPost-jobDetails__employeeNumber">従業員数</dt>
# <dt class="jobOfferPost-jobDetails__workingHours">就業時間</dt>
# <div class="jobOfferPost-jobList" data-jobofferpost-ui="jobList"><div class="mod-jobList-item  jobOfferPost-job" data-joboffermanagementno="e103173151" data-jobofferpost-ui="jobItem">
# <div class="mod-jobList-item  jobOfferPost-job" data-joboffermanagementno="e103173151" data-jobofferpost-ui="jobItem">
# job_list = driver.find_elements_by_class_name("jobOfferPost-jobList")
# job_list = driver.find_elements_by_class_name("mod-jobList-item")
# cur_url = driver.current_url
# print(cur_url)

# 参考サイト
# https://qiita.com/UrTom/items/bcd4d28443826ed92921
# https://qiita.com/memakura/items/20a02161fa7e18d8a693
# https://qiita.com/motoki1990/items/a59a09c5966ce52128be