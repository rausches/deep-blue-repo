import express from 'express';
import analyzeRouter from './controllers/analyzeController.js';

const app = express();
app.use(express.json());
app.use('/analyze', analyzeRouter);

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
    console.log(`Playwright API running on port ${PORT}`);
});
