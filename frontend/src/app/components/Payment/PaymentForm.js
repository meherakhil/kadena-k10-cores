import React, { Component } from 'react';
import TextInput from '../form/TextInput';
import SVG from '../SVG';
import PropTypes from 'prop-types';

class PaymentForm extends Component {
  constructor() {
    super();
    this.getErrorMessage = this.getErrorMessage.bind(this);
  }

  getErrorMessage(field) {
    const { errorField, errorMessage } = this.props;
    return (errorField === field) ? errorMessage : '';
  }

  render() {
    const { number, name, cvc, expiry, changeFieldValue, changeFocus } = this.props;
    const cardNumbersSvgs = <div className="card-payment__icon-block">
      <SVG className='card-payment__icon' name='card-am'/>
      <SVG className='card-payment__icon' name='card-mc'/>
      <SVG className='card-payment__icon' name='card-vs'/>
    </div>;
    const cvcSvgs = <div className="card-payment__icon-block">
      <SVG className='card-payment__icon' name='card'/>
    </div>;

    return (
      <div>
        <div className="card-payment__field">
          <TextInput
            onChange={(e) => { changeFieldValue('number', e.target.value); }}
            changeFocusedField={() => { changeFocus('number'); }}
            label={'Credit card number'}
            value={number}
            labelAnimation={true}
            error={this.getErrorMessage('number')}
            innerElement={cardNumbersSvgs}/>

        </div>

        <div className="card-payment__field">
          <TextInput
            onChange={(e) => { changeFieldValue('name', e.target.value); }}
            changeFocusedField={() => { changeFocus('name'); }}
            label={'Full name'}
            labelAnimation={true}
            error={this.getErrorMessage('name')}
            value={name} />
        </div>

        <div className="card-payment__field card-payment__field--half">
          <TextInput
            onChange={(e) => { changeFieldValue('cvc', e.target.value); }}
            changeFocusedField={() => { changeFocus('cvc'); }}
            label={'CVC'}
            value={cvc}
            labelAnimation={true}
            error={this.getErrorMessage('cvc')}
            innerElement={cvcSvgs} />
        </div>

        <div className="card-payment__field card-payment__field--half">
          <TextInput
            onChange={(e) => { changeFieldValue('expiry', e.target.value); }}
            changeFocusedField={() => { changeFocus('expiry'); }}
            label={'Expiration date'}
            labelAnimation={true}
            error={this.getErrorMessage('expiry')}
            value={expiry} />
        </div>
      </div>
    );
  }
}

PaymentForm.propTypes = {
  number: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  cvc: PropTypes.string.isRequired,
  expiry: PropTypes.string.isRequired,
  changeFieldValue: PropTypes.func.isRequired,
  changeFocus: PropTypes.func.isRequired
};

export default PaymentForm;
