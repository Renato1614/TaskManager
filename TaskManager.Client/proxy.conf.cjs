const apiTarget =
  process.env.services__api__https__0 ||
  process.env.services__api__http__0 ||
  'https://localhost:7222';

module.exports = {
  '/api': {
    target: apiTarget,
    secure: false,
    changeOrigin: true
  }
};
