import requests
import json
import re
import os
import sys
import io

from bs4 import BeautifulSoup

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36"
}

# Parse proxy argument
proxy = None
args = sys.argv[1:]
if len(args) >= 2 and args[0] == '--proxy':
    proxy = args[1]
    args = args[2:]

# Build proxies dict if proxy is set
proxies = None
if proxy:
    proxies = {
        "http": proxy,
        "https": proxy
    }

# Check URL
# url = 'https://bangumi.tv/subject/531159'   # 日々は過ぎれど飯うまし
# url = 'https://bangumi.tv/subject/443428'   # 【推しの子】 第2期

url = args
url = str(url[0])

# Check if the URL is valid
pattern = re.compile(r'^https://bangumi\.tv/subject/\d+/?$')
if not pattern.match(url):
    # print("Error: Invalid URL")
    sys.exit('Invalid URL')

# Get the response from the URL
try:
    response = requests.get(url, headers=headers, timeout=20, proxies=proxies)
    response.raise_for_status()

    if response.text:
        pass
    else:
        # print("Error: No content returned")
        sys.exit('No content returned')

except requests.exceptions.HTTPError as errh:
    # print(errh)
    sys.exit('HTTP Error')
except requests.exceptions.ConnectionError as errc:
    # print(errc)
    sys.exit('Error Connecting')
except requests.exceptions.Timeout as errt:
    # print(errt)
    sys.exit('Timeout Error')
except requests.exceptions.RequestException as err:
    # print(err)
    sys.exit('Something went wrong')

response = requests.get(url, headers=headers, timeout=10, proxies=proxies)
soup = BeautifulSoup(response.content, "lxml")

# Check url is Anime
checkElement = soup.select('#navMenuNeue > li:nth-child(1) > a')
if not checkElement:
    sys.exit('Invalid page structure')
checkElementStr = " ".join(checkElement[0].get('class'))
if checkElementStr != 'focus chl anime':
    # print("Error: This is not an Anime")
    sys.exit('Not an Anime')

# Get the anime Data
animeName = soup.select('#headerSubject > h1 > a')
if animeName:
    animeName = animeName[0].text.strip()
else:
    animeName = "未知标题 ＞︿＜"
# print(animeName)

animeImage = soup.select('#bangumiInfo > div > div:nth-child(1) > a > img')
if animeImage:
    animeImage = f"https:{animeImage[0]['src']}"
else:
    animeImage = "*"
# print(animeImage)

animeInfo = soup.select('#subject_summary')
if animeInfo:
    animeInfo = animeInfo[0].text.strip()
else:
    animeInfo = "找不到简介啊~\nε(┬┬﹏┬┬)3"
# print(animeInfo)

animeNameCn = "*"
animeEpisodes = 0
animeStartDate = "*"
animeWeekday = "*"
animeDirector = "*"
animeWriter = "*"
animeStudio = "*"

animeStaff = soup.select('#infobox > li')
for li in animeStaff:
    text = li.get_text()

    if text.startswith('中文名: '):
        animeNameCn = text.replace('中文名: ', '').strip()

    if text.startswith('话数: '):
        animeEpisodes = text.replace('话数: ', '').strip()
        if type(animeEpisodes) == str:
            try:
                animeEpisodes = int(animeEpisodes)
            except:
                animeEpisodes = 0

    if text.startswith('放送开始: '):
        animeStartDate = text.replace('放送开始: ', '').strip()

    if text.startswith('放送星期: '):
        animeWeekday = text.replace('放送星期: ', '').strip()

    if text.startswith('导演: '):
        animeDirector = text.replace('导演: ', '').strip()

    if text.startswith('脚本: '):
        animeWriter = text.replace('脚本: ', '').strip()

    if text.startswith('动画制作: '):
        animeStudio = text.replace('动画制作: ', '').strip()

# print(animeNameCn)
# print(animeEpisodes)
# print(animeStartDate)
# print(animeWeekday)
# print(animeDirector)
# print(animeWriter)
# print(animeStudio)

animeEpisodesList = soup.select('#subject_detail > div.subject_prg > ul > li')

episodeNow = -1
for li in animeEpisodesList:
    if not li.find('a'):
        break

    epStatus = li.find('a').get('class', [])

    if len(epStatus) > 1 and epStatus[1] == 'epBtnNA':
        break

    episodeNow = li.find('a').text.strip()
    episodeNow = int(episodeNow)

# print(episodeNow)

anime_data = {
    "bangumi_url": url,
    "name": animeName,
    "name_chinese": animeNameCn,
    "image_url": animeImage,
    "summary": animeInfo,
    "total_episodes": animeEpisodes,
    "start_date": animeStartDate,
    "air_weekday": animeWeekday,
    "director": animeDirector,
    "writer": animeWriter,
    "studio": animeStudio,
    "current_episode": episodeNow
}

# 转换为JSON字符串（带缩进、中文不转码）
json_str = json.dumps(anime_data, indent=4, ensure_ascii=False)
print(json_str)