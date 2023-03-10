## 表设计

##### user表

> hash

- user:id
  - name
  - headimg
  - city
  - fragment
  - highest score all time

##### user price表

- price:userid
  - price type1 - 数量
  - price type1 time
  - price type2 - 数量
  - price type2 time

##### price剩余表

> string

- price:type  - 数量

##### city range表

> sort set

- city:id
  - score - userid

## api

##### 创建用户

```
CreateUser(userId) return user
```

##### 设置头像和姓名

```
SetUserInfo(userId,name,headimg) return user
```

##### 查询奖品数量

```
//玩游戏前要判断下（没有了可以提醒用户可以获得排行奖励）
//玩游戏后要判断下(没有了不能抽奖，但是可以参与排行奖励)
GetPriceNumber() return prices
```

##### 保存碎片

```
SaveFragments(userId,fragments)
```

##### 保存积分

```
//最高积分保存到user 根据系统重置时间判断是不是本周
//最高积分保存到对应city
//最高积分保存到city:0 表示全省
SaveScore(userId,city,score,加密)

```

##### 抽奖

```
//根据系统的时间重置点和现在时间以及用户已经中奖情况抽奖
SavePrice(userId,加密) return price?
```

##### 领奖

```
//领完移除
DecrePrice(userId,priceType,加密)
```

##### 设置奖池

```
SetPrices()
```

## 定时任务

- 统计前三
  - 给user price添加数据
  - 队列消息
- 清除周数据
  - 清空city range表
