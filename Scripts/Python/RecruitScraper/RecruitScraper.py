def get_links(url):
    import requests
    from bs4 import BeautifulSoup as soup
    result = requests.get(url)
    page = result.text
    doc = soup(page)
    links = [element.get('href') for element in doc.find_all('a')]
    return links

def get_company(url):
    import requests
    from bs4 import BeautifulSoup as soup
    result = requests.get(url)
    page = result.text
    doc = soup(page)
    links = [element.get('h3 class="jobOfferPost-jobSubTitle"') for element in doc.find_all('a')]
    return links

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

if __name__ == '__main__':
    import time
    import sys

    accountId = sys.argv[1]
    password = sys.argv[2]
    url="https://pdt.r-agent.com/pdt/app/pdt_login_view"

    driver = getChromeDriver()
    driver.get(url)
    time.sleep(5)

    # <input type="text" name="accountId" value="" autocomplete="off" class="mod-textbox">
    acount_box = driver.find_element_by_name("accountId")

    # <input type="password" name="password" value="" class="mod-textbox">
    password_box = driver.find_element_by_name("password")

    # <input type="submit" name="loginButton" value="ログイン" class="mod-button-login">
    login_button = driver.find_element_by_name("loginButton")

    acount_box.send_keys(accountId)
    password_box.send_keys(password)
    time.sleep(1)

    login_button.click()

    # <div class="jobOfferPost-jobList" data-jobofferpost-ui="jobList">
    #   <h3 class="jobOfferPost-jobSubTitle">株式会社</h3>
    # <div class="jobOfferPost-jobDetailsContainer"><h3 class="jobOfferPost-jobDetails__content">仕事の内容</h3>
    # <div class="jobOfferPost-jobDetailsContainer"><h3 class="jobOfferPost-jobDetails__ability">必要な能力・経験</h3>
    # <dt class="jobOfferPost-jobDetails__officeAreas">勤務地</dt>
    # <dt class="jobOfferPost-jobDetails__salary">想定年収</dt>
    # <dt class="jobOfferPost-jobDetails__employeeNumber">従業員数</dt>
    # <dt class="jobOfferPost-jobDetails__workingHours">就業時間</dt>

    cur_url = driver.current_url
    print(cur_url)

    # <div class="jobOfferPost-jobList" data-jobofferpost-ui="jobList"><div class="mod-jobList-item  jobOfferPost-job" data-joboffermanagementno="e103173151" data-jobofferpost-ui="jobItem">
    # <div class="mod-jobList-item  jobOfferPost-job" data-joboffermanagementno="e103173151" data-jobofferpost-ui="jobItem">
    #job_list = driver.find_elements_by_class_name("jobOfferPost-jobList")
    #job_list = driver.find_elements_by_class_name("mod-jobList-item")
    job_list = driver.find_elements_by_class_name("jobOfferPost-job")

    for job in job_list:
        #print(job.text)
        company = job.find_element_by_class_name("jobOfferPost-jobSubTitle")
        dd_list = job.find_elements_by_tag_name("dd")
        dd_str = ''
        for dd in dd_list:
            dd_str+=dd.text
            dd_str+='_'
        print('{}_{}'.format(company.text,dd_str))

 
    '''

    search_box = driver.find_element_by_name("q")
    search_box.send_keys('ChromeDriver')
    search_box.submit()
    '''

    time.sleep(5)
    driver.quit()




'''
    # https://qiita.com/UrTom/items/bcd4d28443826ed92921
    # https://qiita.com/memakura/items/20a02161fa7e18d8a693
    # https://qiita.com/motoki1990/items/a59a09c5966ce52128be
'''

'''
    import requests
    import time
    import sys
    from bs4 import BeautifulSoup

    accountId = sys.argv[1]
    password = sys.argv[2]

    url="https://pdt.r-agent.com/pdt/app/pdt_login_view"

    session = requests.session()
    response = session.get(url)
    response.encoding = response.apparent_encoding

    bs = BeautifulSoup(response.text, 'html.parser')

    login_data = {
        'accountId':accountId,
        'password':password,
    }

    login = session.post(url, data=login_data)
    login.encoding = login.apparent_encoding
    time.sleep(2)

    print(login.text)
'''

