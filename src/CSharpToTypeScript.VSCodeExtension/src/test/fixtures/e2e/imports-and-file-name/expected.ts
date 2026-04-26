import { ExternalModel } from './external-model.model';

export interface OrderDto {
    customer: ExternalModel;
    related: ExternalModel[];
}
