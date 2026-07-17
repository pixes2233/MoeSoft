import sys
import io
import re
import json
import requests
from bs4 import BeautifulSoup
from urllib.parse import quote, urljoin, urlparse

# 解决 Windows 控制台中文（可选）
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

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

# Build proxies dict if proxy is set
# 当 scheme 为 https:// 时，requests 会尝试用 TLS 连接代理，
# 但大多数本地代理（clash/v2ray）端口不支持 TLS。
# 自动将 https:// 转为 http:// 与代理通信（CONNECT 隧道方式）。
proxies = None
if proxy:
    request_proxy = proxy
    parsed = urlparse(proxy)
    if parsed.scheme == 'https':
        request_proxy = f"http://{parsed.hostname}:{parsed.port}"
    proxies = {
        "http": request_proxy,
        "https": request_proxy
    }

if len(args) < 1:
    sys.exit("用法: python \"internet anime.py\" --proxy http://... 关键词")

keyword = args[0]
keyword_encoded = quote(keyword)

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
    "Referer": "https://bangumi.tv/",
}

url = f"https://bangumi.tv/subject_search/{keyword_encoded}?cat=2"

try:
    resp = requests.get(url, headers=headers, timeout=15, proxies=proxies)
    resp.raise_for_status()
except requests.exceptions.InvalidSchema:
    sys.exit("错误: 代理协议不受支持。如果使用了 socks5://，请先安装依赖: pip install requests[socks]")
except requests.exceptions.RequestException as e:
    sys.exit(str(e))

soup = BeautifulSoup(resp.content, "lxml")

items = soup.select("li.item, div.item")
if not items:
    sys.exit("没有找到搜索结果")

results = []

for it in items:
    a_title = it.select_one("h3 a[href*='/subject/']") or it.select_one("h3 a")
    if not a_title:
        continue
    title = a_title.get_text(strip=True)
    link = urljoin("https://bangumi.tv", a_title.get("href", ""))

    img_url = None
    a_cover = it.select_one("a.cover, a.subjectCover, a.subjectCover.cover, a.l")
    if a_cover and a_cover.has_attr("style"):
        m = re.search(r'background-image\s*:\s*url\((["\']?)(.*?)\1\)', a_cover["style"])
        if m:
            raw = m.group(2).strip()
            img_url = urljoin("https:", raw) if raw.startswith("//") else raw

    if not img_url:
        img = it.select_one("img")
        if img:
            raw = None
            if img.get("data-src"):
                raw = img["data-src"]
            elif img.get("srcset"):
                raw = img["srcset"].split(",")[-1].strip().split(" ")[0]
            else:
                raw = img.get("src")
            if raw:
                img_url = urljoin("https:", raw) if raw.startswith("//") else raw

    if not img_url:
        ns_img = it.select_one("noscript img")
        if ns_img:
            raw = ns_img.get("src") or ns_img.get("data-src")
            if raw:
                img_url = urljoin("https:", raw) if raw.startswith("//") else raw

    results.append({
        "title": title,
        "link": link,
        "image": img_url
    })

# 输出 JSON
print(json.dumps(results, ensure_ascii=False))