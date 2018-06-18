import React, { Component } from 'react';
import PropTypes from 'prop-types';
import ImmutablePropTypes from 'react-immutable-proptypes';
import uuid from 'uuid';
import { connect } from 'react-redux';
/* components */
import SVG from 'app.dump/SVG';
/* ac */
import togglePreview from 'app.ac/thumbnailPreview';

class ProductThumbnail extends Component {
  static propTypes = {
    thumbnail: ImmutablePropTypes.mapContains({
      image: PropTypes.string.isRequired,
      border: PropTypes.string,
      magnifyImage: PropTypes.string
    }).isRequired,
    requiresApproval: ImmutablePropTypes.mapContains({
      exists: PropTypes.bool.isRequired,
      text: PropTypes.string.isRequired
    }).isRequired,
    attachments: ImmutablePropTypes.listOf(ImmutablePropTypes.mapContains({
      url: PropTypes.string.isRequired,
      text: PropTypes.string.isRequired
    }).isRequired)
  }

  getAttachmentsComponent = () => {
    const { attachments } = this.props;
    if (!attachments) return null;

    const list = attachments.map((attachment) => {
      return (
        <li key={uuid()}>
          <a
            href={attachment.get('url')}
            className="link link--attachment product-view__attachment"
          >
            <SVG
              name="download--blue"
              className="icon-download"
            />
            <span>{attachment.get('text')}</span>
          </a>
        </li>
      );
    }).toJS();

    return (
      <ul className="product-view__attachments">
        {list}
      </ul>
    );
  };

  render() {
    const { thumbnail, requiresApproval } = this.props;

    const requiresApprovalComponent = requiresApproval.get('exists')
      ? (
        <div className="product-view__flag flag flag--red">
          <SVG
            name="guarantee"
            className="flag__icon"
          />
          <span className="flag__label">{requiresApproval.get('text')}</span>
        </div>
      ) : null;

    return (
      <div className="product-view__img">
        {requiresApprovalComponent}

        <img style={{ border: thumbnail.get('border') }} src={thumbnail.get('image')} />


        <div className="product-view__actions">
          {this.getAttachmentsComponent()}
          <button
            type="button"
            className="product-view__zoom"
            onClick={() => this.props.togglePreview(thumbnail.get('magnifyImage'))}
          >
            <SVG
              name="zoom"
              className="icon-zoom"
            />
          </button>
        </div>
      </div>
    );
  }
}

export default connect(null, {
  togglePreview
})(ProductThumbnail);
