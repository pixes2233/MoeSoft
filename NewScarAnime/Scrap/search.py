import sys
import json


def main():

    keyword = sys.argv[1]

    result = {
        "keyword": keyword,
        "status": "success",
        "message": "Python调用成功"
    }

    print(
        json.dumps(
            result,
            ensure_ascii=False
        )
    )


if __name__ == "__main__":
    main()