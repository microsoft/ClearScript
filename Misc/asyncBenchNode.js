console.time('time');

const N = 5 * 1000 * 1000;
const primes = [2];

async function is_prime(x) {
    for (let i = 0; primes[i] * primes[i] <= x; ++i) {
        if (x % primes[i] === 0) {
            return false;
        }
    }
    return true;
}

(async function() {
    for (let x = 3; primes.length < N; ++x) {
        if (await is_prime(x)) {
            primes.push(x);
        }
    }
    console.log(primes[primes.length - 1]);
    console.timeEnd('time');
})();
