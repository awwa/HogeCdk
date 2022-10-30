# CDKサンプルプロジェクト

## はじめに
このサンプルはCDKを使ったAWSリソースのデプロイのサンプル集です。  
あくまでもサンプルを集めただけなので、間違ってもこのサンプルの構成のままスタックを分離してはいけません。  
構築するシステムごとに以下の事項を検討し、それに合わせて構成を変更すること。  

## 実装時に考慮すること
- AWSアカウントの分離戦略
    - 開発環境と本番環境
    - システムの種類
    - テナント単位
- AWSアカウント内のワークロード分離戦略
    - テナントごと
    - システムの種類ごと
- CloudFormation内のスタック分離戦略
    - 共通基盤
    - テナントごと
    - ステートフルとステートレス
    - ライフサイクル

## 参考サイト
迷ったら以下のドキュメントを読むこと。
- [コードによる管理がどうだったら良さそうか](https://qiita.com/yktko/items/da8d5ae9a540d5427b01)
- [AWS CDKにおけるスタックの分割の検討](https://zenn.dev/intercept6/articles/aws-cdk-stack-div)

## CDKのサンプルコードが生成した内容

* `dotnet build src` compile this app
* `cdk ls`           list all stacks in the app
* `cdk synth`       emits the synthesized CloudFormation template
* `cdk deploy`      deploy this stack to your default AWS account/region
* `cdk diff`        compare deployed stack with current state
* `cdk docs`        open CDK documentation

Enjoy!
