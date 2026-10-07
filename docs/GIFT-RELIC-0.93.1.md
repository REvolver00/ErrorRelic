# Campfire Gift Relic — 0.93.1 experimental integration

Co-op only. Gift is a native rest-site action: choose one relic and a teammate, transfer directly without recipient confirmation, and return success only after the transfer and obtain effects finish on each peer. ERROR PROOF remains non-transferable. ERROR H/E identity and discovery state are preserved through relic serialization.

## Fix: native completion and UI lifecycle

The 2026-10-05 Gift2P log shows `RestSiteOption.Icon` throwing in `NRestSiteCharacter.ShowSelectedRestSiteOption`, called by `RestSiteSynchronizer.AfterPlayerOptionChosen`. That exception interrupts `ChooseOption` before normal option consumption and prevents `NRestSiteButton.SelectOption` from calling `AfterSelectingOption`. Its failure cleanup then re-enables old buttons. The former `BeforeLocalRestSiteExited()` call also cleared the options before this notification, leaving live buttons pointing at an empty list.

Gift now returns through the same native success path as Heal/Smith/Mend. It does not call the room-exit method, mutate rest-site option lists, patch the consumption hook, or maintain a Gift-used counter. The native sequence notifies the UI, clears the acting player's options under the standard rest-site rule, completes that player's rest site, and lets the button run the room's normal hide/rebuild/proceed flow. The teammate retains their own action. Native extra-action modifiers retain their normal semantics, as with Heal/Smith.

One `PlayerChoiceSynchronizer` choice carries `[source relic index, target player slot]`; an empty list cancels. The owner always settles that choice even if local selection fails. Both peers execute and await `RelicCmd.Remove` and `RelicCmd.Obtain` inside `OnSelect`, including nested obtain-time choices. The old independent Gift message handler and its unawaited transfer are removed. The picker is detached before targeting and settles cancellation when removed from the scene tree.

## Assets

The non-virtual native icon getter requires `res://images/ui/rest_site/option_errorrelics_gift_relic.png`. That source PNG reuses the existing ERROR relic artwork. `tools/pack-assets.gd` includes the native rest-site image directory and emits its texture/remap into the PCK. Both localizations include the native `OPTION_ERRORRELICS_GIFT_RELIC.name` title key.

## Test DLL and remaining acceptance

The existing Gift2P launcher intentionally deletes `ErrorRelics.Tests.dll` and does not pass `--errorrelic-smoke`. The existing `MultiplayerSmokeDriver` only automatically grants Proof when its smoke mode is enabled. Restoring the DLL alone therefore does not make Gift2P grant Proof, and its next launch deletes the DLL again. The launcher, BAT and test-driver sources are unchanged.

Real two-window acceptance remains necessary: Host→Client and Client→Host Gift, teammate Smith/Heal, cancel/retry, hover over the former button location, and room transition with matching inventories and no state divergence. Include an ERROR relic whose obtain effect asks for a choice. Both peers must use the same updated DLL/PCK because the former custom Gift message protocol has been removed.

## Validation — 2026-10-07

- Debug build succeeds; the existing console suite passes 62,702 checks.
- A temporary console fixture in `.godot/gift-validation` passes 56 targeted checks using the installed game's real `RestSiteSynchronizer.ChooseOption` and `PlayerChoiceSynchronizer`. It covers both sender directions, awaited completion, notification-before-consumption order, independent teammate options, cancellation/failure settlement and retry, ERROR state serialization, no pending Gift choice, and a subsequent DeckCard choice. Picker/target UI, transport and RelicCmd effects are mocked; this is not a live Gift UI acceptance test.
- The installed game engine loads the generated PCK and resolves the native Gift icon as a 256×256 texture; both language title keys are present.
- The unchanged existing multiplayer smoke workflow passes on Host (1) and Client (1001), including its Proof-grant setup, three combats and treasure award/inventory checks. The test DLL loads on both peers. Headless rendering emits dummy-renderer RID/shader warnings; no state divergence or managed exception was reported in this smoke run.
- Updated DLL/PCK are deployed to the local game mod directory and existing isolated multiplayer runtime. Test DLL deployment remains confined to that isolated runtime. No Steam upload was performed.

## 联机生成筛选配置不一致 — 2026-10-07

本次补充覆盖两个生成开关：`AllowAncientSourceEffects`（允许先古来源）与 `AllowMultiplayerImpactEffects`（允许可能影响联机流程的 Effect）。此前各客户端独立读取本地设置：相同随机种子不代表相同候选池。复现脚本中，一端禁用先古来源、另一端禁用联机影响，1,000 次生成有 880 次 H/E 不一致，两端 RNG 计数却同为 2,000。

用户确认的规则：联机整局使用房主开局的生成筛选设置，双方本地偏好保留。进入第一张地图、执行地图与房间生成逻辑之前，通过原版 `PlayerChoiceSynchronizer` 同步房主的两个开关，随后从本局状态读取筛选规则；不按玩家各自的本地设置生成，也不合并双方限制。同步使用网络提供的实际房主 ID，不假定第一个玩家必定是房主。

本局设置以 `ErrorRelics.GenerationFilters` 随运行状态保存并参与网络序列化。读档沿用已保存的设置；缺少该字段的旧存档，在升级后第一次进入地图时采用当时房主的设置并保存。局中修改本地开关留到下一次新联机局生效。单人游戏仍跟随当前本地开关，已有遗物不重生成。两个开关的中英文悬浮说明已补充这些规则。

| 房主配置 | 客户端配置 | 本局实际规则 |
| --- | --- | --- |
| 禁用先古来源，允许联机影响 | 允许先古来源，禁用联机影响 | 禁用先古来源，允许联机影响 |
| 允许先古来源，禁用联机影响 | 禁用先古来源，允许联机影响 | 允许先古来源，禁用联机影响 |

临时定向检查位于 `.godot/filter-validation`，不修改或加入现有 BAT/测试器：全部 16 种开关组合 × 两种房主位置，共 32,768 对实际生成函数的结果比较与 98,691 项断言通过。覆盖房主规则生效、客户端本地偏好保留、局中设置变化不影响本局、单人语义保留、JSON/网络保存值往返、旧存档缺字段，以及重复调用不会重复预留配置 choice。该定向检查模拟网络传递，但使用实际原版 choice 同步器和本 Mod 生成器。

实机回归使用未改动的 `--errorrelic-smoke` 流程：分别启动 Host/Client，在各进程初始化时提供上述相反配置，之后恢复测试目录原配置文件。比较两端 `ERROR_MP_GENERATION` 和 `ERROR_MP_CIRCLET` 记录，检查有效配置日志、双方 smoke 结果和 state divergence。日志保存在 `.godot/filter-validation/host2-client1-*` 与 `host1-client2-*`；此项验证生成配置一致性，不替代上一节仍待进行的 Gift 双窗口交互验收。

两轮实机结果均通过：每轮 Host/Client 都出现 ERROR_MP_SMOKE_PASS，每轮 18 条生成记录逐条一致，两端均锁定房主配置，未发现 state divergence 或托管异常。测试后已恢复原测试配置文件，并退出本次启动的后台游戏进程。
