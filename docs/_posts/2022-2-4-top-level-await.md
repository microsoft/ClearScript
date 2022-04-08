---
title: Top-Level Await in ClearScript 7.2.2
---
ClearScript 7.2.2 is paired with V8 9.8, which no longer supports Top-Level Await control.

# Background

[Top-Level Await](https://github.com/tc39/proposal-top-level-await) is a feature that enables code at the outermost scope of an [ECMAScript 6](https://262.ecma-international.org/6.0/#sec-modules) module to be executed as an [async function](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function).

ClearScript 7.1.3 added an [API](https://microsoft.github.io/ClearScript/Reference/html/P_Microsoft_ClearScript_V8_V8Settings_EnableTopLevelAwait.htm) for controlling Top-Level Await. When enabled, it caused module evaluation to return a [promise](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise), eventually to be resolved with the module's evaluation result.

Unfortunately, due to a [V8 bug](https://bugs.chromium.org/p/v8/issues/detail?id=11715), the promise resolution value is always [undefined](https://developer.mozilla.org/en-US/docs/Glossary/undefined). For that reason, and to maintain compatibility with hosts that rely on _immediate_ module evaluation results, ClearScript left Top-Level Await disabled by default.

# V8 9.8 and ClearScript 7.2.2

As of version 9.8, V8 no longer allows embedders to control Top-Level Await. Instead, the feature is always enabled.

However, embedders can now ask V8 whether a loaded module requires _async evaluation_ – true only if the module uses [`await`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/await) or [`for await...of`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for-await...of), or if any of its imported modules require async evaluation.

ClearScript 7.2.2 uses this information to extract the correct result from a promise tracking normal (non-async) module evaluation, and to return that result directly to the host.

That preserves ClearScript's original behavior and allows async modules to work as expected. The only remaining issue, as of this blog entry, is that the evaluation result of an async module is still inaccessible due to the V8 bug mentioned above.

Good luck!