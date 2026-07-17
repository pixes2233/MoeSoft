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

# Validate proxy: must include protocol prefix (http://, https://, socks4://, socks5://, socks5h://)
if proxy:
    if not re.match(r'^(https?|socks[45]h?):\/\/', proxy):
        sys.exit("错误: --proxy 参数必须包含协议前缀，例如 http://、https://、socks5:// 等")

import urllib.parse

def build_proxies(proxy_url):
    """构建 proxies 字典"""
    return {
        "http": proxy_url,
        "https": proxy_url
    }

def fetch_with_auto_fallback(url, headers, timeout, proxy=None):
    """
    尝试获取 URL。
    对于 https:// 代理前缀，requests 会尝试用 TLS 连接代理，
    但大多数本地代理（clash/v2ray）端口不支持 TLS。
    因此自动将 https:// 转为 http:// 与代理通信（CONNECT 隧道方式）。
    """
    if not proxy:
        response = requests.get(url, headers=headers, timeout=timeout)
        response.raise_for_status()
        return response

    # 如果 scheme 是 https://，转为 http://
    #（requests 的 https:// 代理意味着"用 TLS 连接代理"，
    #  但本地代理端口一般不支持 TLS，用户本意是用 HTTP 代理）
    parsed = urllib.parse.urlparse(proxy)
    request_proxy = proxy
    if parsed.scheme == 'https':
        request_proxy = f"http://{parsed.hostname}:{parsed.port}"

    proxies = build_proxies(request_proxy)

    try:
        response = requests.get(url, headers=headers, timeout=timeout, proxies=proxies)
        response.raise_for_status()
        if not response.text:
            sys.exit('No content returned')
        return response

    except requests.exceptions.InvalidSchema:
        sys.exit("错误: 代理协议不受支持。如果使用了 socks5://，请先安装依赖: pip install requests[socks]")
    except requests.exceptions.HTTPError as errh:
        sys.exit('HTTP Error')
    except requests.exceptions.ConnectionError as errc:
        sys.exit('Error Connecting')
    except requests.exceptions.Timeout as errt:
        sys.exit('Timeout Error')
    except requests.exceptions.RequestException as err:
        sys.exit('Something went wrong')

# Check URL
url = args
url = str(url[0])

# Check if the URL is valid
pattern = re.compile(r'^https://bangumi\.tv/subject/\d+/?$')
if not pattern.match(url):
    sys.exit('Invalid URL')

# Fetch with proxy auto-detection
response = fetch_with_auto_fallback(url, headers=headers, timeout=10, proxy=proxy)
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